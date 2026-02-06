using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void PosaljiTekst(Socket s, string msg)
        {
            try { SendText(s, msg); }
            catch { }
        }

        private static void ObradiPoruku(string poruka, Socket klijent)
        {
            if (potezZavrsen.ContainsKey(klijent) && potezZavrsen[klijent])
            {
                Console.WriteLine("Igrač je već završio potez.");
                return;
            }

            lock (lockObject)
            {
                // === ZAMENA_SYNC ===
                if (poruka.StartsWith("ZAMENA_SYNC "))
                {
                    if (tcpKlijenti.Count < 2)
                    {
                        try { SendText(klijent, "ZAMENA_SYNC_FAIL SAMO_JEDAN_IGRAC"); } catch { }
                        return;
                    }

                    if (zamenaSyncDone.Count >= 2 && !zamenaSyncDone.Contains(klijent))
                    {
                        try { SendText(klijent, "ZAMENA_SYNC_FAIL VEC_KORISCENA"); } catch { }
                        return;
                    }

                    string[] delovi = poruka.Split(' ', 3);
                    if (delovi.Length < 3)
                    {
                        try { SendText(klijent, "ZAMENA_SYNC_FAIL LOS_FORMAT"); } catch { }
                        return;
                    }

                    try
                    {
                        KARTA dataKarta = DeserializeFromBase64<KARTA>(delovi[1]);
                        KARTA dobijenaKarta = DeserializeFromBase64<KARTA>(delovi[2]);

                        if (!karteIgraca.ContainsKey(klijent))
                        {
                            try { SendText(klijent, "ZAMENA_SYNC_FAIL NEPOZNAT_IGRAC"); } catch { }
                            return;
                        }

                        KARTA zaUkloniti = karteIgraca[klijent].FirstOrDefault(k => k.Naziv == dataKarta.Naziv);
                        if (zaUkloniti == null)
                        {
                            try { SendText(klijent, "ZAMENA_SYNC_FAIL DATA_KARTA_NIJE_NADJENA"); } catch { }
                            return;
                        }

                        karteIgraca[klijent].Remove(zaUkloniti);
                        karteIgraca[klijent].Add(dobijenaKarta);

                        zamenaSyncDone.Add(klijent);
                        zamenaVecIskoriscenaUTomPotezu = true;

                        try { SendText(klijent, "ZAMENA_SYNC_OK"); } catch { }
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška u ZAMENA_SYNC: {ex.Message}");
                        try { SendText(klijent, "ZAMENA_SYNC_FAIL DESER_GRESKA"); } catch { }
                    }

                    return;
                }

                // === ZAMENA_ZAHTEV (TCP ponuda) ===
                if (poruka.StartsWith("ZAMENA_ZAHTEV"))
                {
                    if (zamenaVecIskoriscenaUTomPotezu)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA VEC_KORISCENA"); } catch { }
                        return;
                    }
                    if (zamenaUToku)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA U_TOKU"); } catch { }
                        return;
                    }

                    string[] delovi = poruka.Split(' ', 3);
                    if (delovi.Length < 3)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA LOS_FORMAT"); } catch { }
                        return;
                    }

                    string imeKarte = delovi[1];

                    if (!int.TryParse(delovi[2], out int portPrimaoca))
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA LOS_PORT"); } catch { }
                        return;
                    }

                    Socket primalac = tcpKlijenti.FirstOrDefault(s => ((IPEndPoint)s.RemoteEndPoint).Port == portPrimaoca);

                    if (primalac == null)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA PRIMALAC_NE_POSTOJI"); } catch { }
                        return;
                    }

                    if (!karteIgraca.ContainsKey(klijent))
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA NEMAS_RUKU"); } catch { }
                        return;
                    }

                    var karta = karteIgraca[klijent].FirstOrDefault(k => k.Naziv == imeKarte);
                    if (karta == null)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA NEMAS_KARTU"); } catch { }
                        return;
                    }

                    if (primalac == klijent)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA NE_MOZES_SEBI"); } catch { }
                        return;
                    }

                    zamenaUToku = true;
                    zamenaOd = klijent;
                    zamenaKa = primalac;
                    kartaZaZamenu = karta;

                    string zahtev = $"ZAMENA_PONUDA {karta.Naziv}";
                    try { SendText(primalac, zahtev); } catch { }

                    return;
                }

                if (poruka == "ZAMENA_PRIHVATI" && klijent == zamenaKa)
                {
                    karteIgraca[zamenaOd].Remove(kartaZaZamenu);
                    karteIgraca[zamenaKa].Add(kartaZaZamenu);

                    zamenaVecIskoriscenaUTomPotezu = true;

                    try { SendText(zamenaOd, "ZAMENA_USPESNA"); } catch { }
                    try { SendText(zamenaKa, "ZAMENA_USPESNA"); } catch { }

                    zamenaUToku = false;
                    zamenaOd = null;
                    zamenaKa = null;
                    kartaZaZamenu = null;

                    return;
                }

                if (poruka == "ZAMENA_ODBIJ" && klijent == zamenaKa)
                {
                    try { SendText(zamenaOd, "ZAMENA_ODBIJENA"); } catch { }

                    zamenaUToku = false;
                    zamenaOd = null;
                    zamenaKa = null;
                    kartaZaZamenu = null;

                    return;
                }

                if (poruka == "ZAMENA")
                {
                    if (zamenaUToku)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA U_TOKU"); } catch { }
                        return;
                    }

                    if (tcpKlijenti.Count < 2)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA SAMO_JEDAN_IGRAC"); } catch { }
                        return;
                    }

                    if (zamenaVecIskoriscenaUTomPotezu)
                    {
                        try { SendText(klijent, "ZAMENA_ODBIJENA VEC_KORISCENA"); } catch { }
                        return;
                    }

                    try { SendText(klijent, "ZAMENA_DOZVOLJENA"); } catch { }
                    return;
                }

                if (poruka.StartsWith("IZBACUJEM"))
                {
                    if (kartuOdigrao.ContainsKey(klijent) && kartuOdigrao[klijent])
                    {
                        Console.WriteLine("Ne može izbacivanje – karta je već odigrana u ovom potezu.");
                        return;
                    }

                    if (kartaIzbacena.ContainsKey(klijent) && kartaIzbacena[klijent])
                    {
                        Console.WriteLine("Igrač je već izbacio kartu u ovom potezu.");
                        return;
                    }

                    string imeKarte = poruka.Substring(10).Trim();
                    Console.WriteLine($"Klijent {klijent.RemoteEndPoint} izbacuje kartu: {imeKarte}");

                    if (karteIgraca.ContainsKey(klijent))
                    {
                        KARTA karta = karteIgraca[klijent].FirstOrDefault(k => k.Naziv == imeKarte);

                        if (karta != null)
                        {
                            karteIgraca[klijent].Remove(karta);
                            kartaIzbacena[klijent] = true;

                            Console.WriteLine($"Karta {imeKarte} uklonjena iz ruke igrača.");
                        }
                        else
                        {
                            Console.WriteLine($"Karta {imeKarte} nije pronađena u ruci igrača.");
                        }
                    }

                    return;
                }

                // ODIGRAO ...  (ovo je najduži deo; ostavljen identično tvojoj logici)
                if (poruka.StartsWith("ODIGRAO"))
                {
                    HandleOdigrao(poruka, klijent);
                    return;
                }

                if (poruka.StartsWith("ZAVRSIO_POTEZ"))
                {
                    HandleZavrsioPotez(poruka, klijent);
                    return;
                }

                if (poruka == "NOVI_POTEZ_POTVRDA")
                {
                    Console.WriteLine($"Klijent {klijent.RemoteEndPoint} je spreman za novi potez.");
                    return;
                }

                Console.WriteLine($"Nepoznata poruka od klijenta: {poruka}");
            }
        }
    }
}
