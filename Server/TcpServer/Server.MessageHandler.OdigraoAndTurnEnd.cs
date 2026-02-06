using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void HandleZavrsioPotez(string poruka, Socket klijent)
        {
            if (potezZavrsen.ContainsKey(klijent) && potezZavrsen[klijent])
                return;

            string[] delovi = poruka.Split(' ');
            int koliko = 0;

            if (delovi.Length == 2)
            {
                koliko = int.Parse(delovi[1]);
            }
            else
            {
                if (karteIgraca.ContainsKey(klijent))
                {
                    int trenutnoKarata = karteIgraca[klijent].Count;
                    int potrebnoKarata = (brojIgraca == 3) ? 4 : 5;
                    koliko = Math.Max(0, potrebnoKarata - trenutnoKarata);
                }
            }

            kartuOdigrao[klijent] = false;
            potezZavrsen[klijent] = true;
            trazeneKarte[klijent] = koliko;

            Console.WriteLine($"Klijent {klijent.RemoteEndPoint} završio potez, traži {koliko} karata.");
        }

        private static void HandleOdigrao(string poruka, Socket klijent)
        {
            if (kartuOdigrao.ContainsKey(klijent) && kartuOdigrao[klijent])
            {
                Console.WriteLine($"Klijent {klijent.RemoteEndPoint} je već odigrao kartu u ovom potezu.");
                return;
            }

            string[] delovi = poruka.Split(' ');
            if (delovi.Length < 3)
            {
                Console.WriteLine($"Neispravna poruka za odigravanje: {poruka}");
                return;
            }

            string drugiDeo = delovi[1];
            bool jeCiglaMalter = drugiDeo.Contains("Cigla") || drugiDeo.Contains("Malter");

            if (jeCiglaMalter && delovi.Length >= 3)
            {
                int trakaBroj = int.Parse(delovi[^1]);
                string imeKarte = string.Join(" ", delovi, 1, delovi.Length - 2);

                TRAKA traka = trake.FirstOrDefault(t => t.Broj == trakaBroj);
                if (traka == null)
                {
                    Console.WriteLine($"Traka {trakaBroj} nije pronađena.");
                    return;
                }

                bool jeLjubicasta = imeKarte.Contains("ljubicasta");

                if (!jeLjubicasta)
                {
                    if (imeKarte.Contains("plava") && traka.Boja != TRAKA.BOJA.plava ||
                        imeKarte.Contains("zelena") && traka.Boja != TRAKA.BOJA.zelena ||
                        imeKarte.Contains("crvena") && traka.Boja != TRAKA.BOJA.crvena)
                    {
                        Console.WriteLine("Nevalidan potez – boja karte ne odgovara boji trake.");
                        return;
                    }
                }

                if (!karteIgraca.ContainsKey(klijent) ||
                    !karteIgraca[klijent].Any(k => k.Naziv.Contains(imeKarte)))
                {
                    Console.WriteLine($"Nevalidan potez – igrač nema kartu {imeKarte}.");
                    return;
                }

                if (traka.brojZidina < 2)
                {
                    traka.brojZidina++;
                    Console.WriteLine($"Traka {trakaBroj} sada ima {traka.brojZidina} zidina.");
                }
                else
                {
                    Console.WriteLine($"Traka {trakaBroj} već ima maksimalan broj zidina (2).");
                }

                if (karteIgraca.ContainsKey(klijent))
                {
                    var kartaZaUklanjanje = karteIgraca[klijent].FirstOrDefault(k =>
                        k.Naziv.Contains(imeKarte) ||
                        (imeKarte.Contains("Cigla") && k.Naziv.Contains("Cigla")) ||
                        (imeKarte.Contains("Malter") && k.Naziv.Contains("Malter")));

                    if (kartaZaUklanjanje != null)
                    {
                        karteIgraca[klijent].Remove(kartaZaUklanjanje);
                        Console.WriteLine($"Karta {kartaZaUklanjanje.Naziv} uklonjena iz ruke igrača.");
                    }
                    else
                    {
                        Console.WriteLine($"Nije pronađena karta {imeKarte} u ruci igrača.");
                    }
                }

                if (kartuOdigrao.ContainsKey(klijent))
                    kartuOdigrao[klijent] = true;

                return;
            }
            else if (drugiDeo == "Cigla_Malter" && delovi.Length == 3)
            {
                int trakaBroj = int.Parse(delovi[2]);
                TRAKA traka = trake.FirstOrDefault(t => t.Broj == trakaBroj);
                if (traka != null)
                {
                    if (traka.brojZidina < 2)
                    {
                        traka.brojZidina = Math.Min(2, traka.brojZidina + 1);
                        Console.WriteLine($"Traka {trakaBroj} sada ima {traka.brojZidina} zidina (Cigla+Malter).");
                    }
                    else
                    {
                        Console.WriteLine($"Traka {trakaBroj} već ima maksimalan broj zidina (2).");
                    }
                }

                if (karteIgraca.ContainsKey(klijent))
                {
                    var cigla = karteIgraca[klijent].FirstOrDefault(k => k.Naziv.Contains("Cigla"));
                    var malter = karteIgraca[klijent].FirstOrDefault(k => k.Naziv.Contains("Malter"));

                    List<string> uklonjeneKarte = new List<string>();

                    if (cigla != null)
                    {
                        karteIgraca[klijent].Remove(cigla);
                        uklonjeneKarte.Add(cigla.Naziv);
                    }
                    if (malter != null)
                    {
                        karteIgraca[klijent].Remove(malter);
                        uklonjeneKarte.Add(malter.Naziv);
                    }

                    if (uklonjeneKarte.Count > 0)
                        Console.WriteLine($"Karte {string.Join(" + ", uklonjeneKarte)} uklonjene iz ruke igrača (Cigla+Malter kombinacija).");
                    else
                        Console.WriteLine("Nije pronađena Cigla i/ili Malter u ruci igrača.");
                }

                if (kartuOdigrao.ContainsKey(klijent))
                    kartuOdigrao[klijent] = true;

                return;
            }
            else
            {
                if (delovi.Length < 5)
                {
                    Console.WriteLine($"Neispravan broj parametara za napad: {poruka}");
                    return;
                }

                int brojDelova = delovi.Length;
                string imeKarte = string.Join(" ", delovi, 1, brojDelova - 4);

                if (!int.TryParse(delovi[brojDelova - 3], out int trakaBroj) ||
                    !int.TryParse(delovi[brojDelova - 2], out int zona) ||
                    !int.TryParse(delovi[brojDelova - 1], out int protivnikRedniBroj))
                {
                    Console.WriteLine($"Neispravni parametri (nisu brojevi): traka={delovi[brojDelova - 3]}, zona={delovi[brojDelova - 2]}, protivnik={delovi[brojDelova - 1]}");
                    return;
                }

                TRAKA traka = trake.FirstOrDefault(t => t.Broj == trakaBroj);
                if (traka == null)
                {
                    Console.WriteLine($"Traka {trakaBroj} nije pronađena.");
                    return;
                }

                List<PROTIVNIK> ciljanaZona = null;
                string nazivZone = "";
                switch (zona)
                {
                    case 1: ciljanaZona = traka.strelacZona; nazivZone = "strelac"; break;
                    case 2: ciljanaZona = traka.vitezZona; nazivZone = "vitez"; break;
                    case 3: ciljanaZona = traka.macevalacZona; nazivZone = "mačevalac"; break;
                    case 4: ciljanaZona = traka.suma; nazivZone = "šuma"; break;
                    default:
                        Console.WriteLine($"Nepoznata zona: {zona}");
                        return;
                }

                if (zona == 4)
                {
                    Console.WriteLine("Nevalidan potez – napadne karte se ne mogu igrati u šumi.");
                    return;
                }

                if (imeKarte.Contains("Strelac") && zona != 1 ||
                    imeKarte.Contains("Vitez") && zona != 2 ||
                    imeKarte.Contains("Macevalac") && zona != 3)
                {
                    Console.WriteLine("Nevalidan potez – karta se ne može igrati u toj zoni.");
                    return;
                }

                if (protivnikRedniBroj < 1 || protivnikRedniBroj > ciljanaZona.Count)
                {
                    Console.WriteLine($"Protivnik {protivnikRedniBroj} nije pronađen u zoni {zona} trake {trakaBroj} (ima {ciljanaZona.Count} protivnika).");
                    return;
                }

                PROTIVNIK protivnik = ciljanaZona[protivnikRedniBroj - 1];

                if (imeKarte.Contains("Varvarin"))
                {
                    ciljanaZona.RemoveAt(protivnikRedniBroj - 1);
                    Console.WriteLine($"Protivnik {protivnik.Ime} ELIMINISAN od Varvarina u zoni {nazivZone} trake {trakaBroj}!");
                }
                else if (imeKarte.Contains("Heroj"))
                {
                    int prethodniHP = protivnik.ZivotniPoeni;
                    protivnik.ZivotniPoeni--;
                    Console.WriteLine($"Heroj udara {protivnik.Ime} u zoni {nazivZone} trake {trakaBroj} HP: {prethodniHP} -> {protivnik.ZivotniPoeni}");

                    if (protivnik.ZivotniPoeni <= 0)
                    {
                        ciljanaZona.RemoveAt(protivnikRedniBroj - 1);
                        Console.WriteLine($"Protivnik {protivnik.Ime} uništen od Heroja!");
                    }
                }
                else if (imeKarte.Contains("Katran"))
                {
                    if (!protivniciZalepljeniKatranom.ContainsKey(protivnik))
                    {
                        protivniciZalepljeniKatranom[protivnik] = true;
                        Console.WriteLine($"Protivnik {protivnik.Ime} zalepljen katranom u zoni {nazivZone} trake {trakaBroj} - neće se pomerati!");
                    }
                    else if (!protivniciZalepljeniKatranom[protivnik])
                    {
                        protivniciZalepljeniKatranom[protivnik] = true;
                        Console.WriteLine($"Protivnik {protivnik.Ime} zalepljen katranom u zoni {nazivZone} trake {trakaBroj} - neće se pomerati!");
                    }
                    else
                    {
                        Console.WriteLine($"Protivnik {protivnik.Ime} je već zalepljen katranom.");
                    }
                }
                else
                {
                    int prethodniHP = protivnik.ZivotniPoeni;
                    protivnik.ZivotniPoeni--;
                    Console.WriteLine($"Karta {imeKarte} udara {protivnik.Ime} u zoni {nazivZone} trake {trakaBroj} HP: {prethodniHP} -> {protivnik.ZivotniPoeni}");

                    if (protivnik.ZivotniPoeni <= 0)
                    {
                        ciljanaZona.RemoveAt(protivnikRedniBroj - 1);
                        Console.WriteLine($"Protivnik {protivnik.Ime} je uništen.");
                    }
                }

                if (karteIgraca.ContainsKey(klijent))
                {
                    var kartaZaUklanjanje = karteIgraca[klijent].FirstOrDefault(k => k.Naziv == imeKarte);
                    if (kartaZaUklanjanje == null)
                    {
                        kartaZaUklanjanje = karteIgraca[klijent].FirstOrDefault(k =>
                            k.Naziv.Contains(imeKarte) ||
                            imeKarte.Contains(k.Naziv.Split(' ')[0]));
                    }

                    if (kartaZaUklanjanje != null)
                    {
                        karteIgraca[klijent].Remove(kartaZaUklanjanje);
                        Console.WriteLine($"Karta {kartaZaUklanjanje.Naziv} uklonjena iz ruke igrača.");
                    }
                    else
                    {
                        Console.WriteLine($"Nije pronađena karta '{imeKarte}' u ruci igrača.");
                    }
                }

                if (kartuOdigrao.ContainsKey(klijent))
                    kartuOdigrao[klijent] = true;
            }
        }
    }
}
