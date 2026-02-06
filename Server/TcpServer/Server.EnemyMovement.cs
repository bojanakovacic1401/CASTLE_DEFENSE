using System;
using System.Collections.Generic;
using System.Linq;
using Biblioteka;

using System.Net.Sockets;
namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static bool JeSpecijalan(PROTIVNIK p)
        {
            if (p == null) return false;

            return p.Ime.StartsWith("Pomeranje")
                || p.Ime == "Kuga! Strelci"
                || p.Ime == "Postavi još četiri protivnika"
                || p.Ime == "Prelaze u sledeću traku";
        }

        private static void PomeriProtivnike()
        {
            Console.WriteLine("\n=== POMERANJE PROTIVNIKA ===");

            foreach (TRAKA traka in trake)
            {
                var sumaSnap = traka.suma.ToList();
                var strelacSnap = traka.strelacZona.ToList();
                var vitezSnap = traka.vitezZona.ToList();
                var maceSnap = traka.macevalacZona.ToList();

                foreach (var p in sumaSnap)
                {
                    if (JeSpecijalan(p))
                    {
                        ObradiSpecijalnogProtivnika(p, traka);
                        traka.suma.Remove(p);
                    }
                }
                foreach (var p in strelacSnap)
                {
                    if (JeSpecijalan(p))
                    {
                        ObradiSpecijalnogProtivnika(p, traka);
                        traka.strelacZona.Remove(p);
                    }
                }
                foreach (var p in vitezSnap)
                {
                    if (JeSpecijalan(p))
                    {
                        ObradiSpecijalnogProtivnika(p, traka);
                        traka.vitezZona.Remove(p);
                    }
                }
                foreach (var p in maceSnap)
                {
                    if (JeSpecijalan(p))
                    {
                        ObradiSpecijalnogProtivnika(p, traka);
                        traka.macevalacZona.Remove(p);
                    }
                }
            }

            foreach (TRAKA traka in trake)
            {
                PremestiListuBezKatrana(traka.vitezZona, traka.macevalacZona, "vitez", "mačevalac", traka.Broj);
                PremestiListuBezKatrana(traka.strelacZona, traka.vitezZona, "strelac", "vitez", traka.Broj);
                PremestiListuBezKatrana(traka.suma, traka.strelacZona, "šuma", "strelac", traka.Broj);

                for (int i = traka.macevalacZona.Count - 1; i >= 0; i--)
                {
                    PROTIVNIK p = traka.macevalacZona[i];

                    if (protivniciZalepljeniKatranom.TryGetValue(p, out bool zalepljen) && zalepljen)
                    {
                        protivniciZalepljeniKatranom[p] = false;
                        Console.WriteLine($"{p.Ime} je zalepljen katranom u traci {traka.Broj} i preskače kraj poteza.");
                        continue;
                    }

                    if (traka.brojZidina > 0)
                    {
                        int hpPre = p.ZivotniPoeni;
                        p.ZivotniPoeni--;

                        int zidPre = traka.brojZidina;
                        traka.brojZidina = Math.Max(0, traka.brojZidina - 1);

                        Console.WriteLine($"{p.Ime} udara u zidinu u traci {traka.Broj}! HP: {hpPre}->{p.ZivotniPoeni}, Zidine: {zidPre}->{traka.brojZidina}");

                        if (p.ZivotniPoeni <= 0)
                        {
                            traka.macevalacZona.RemoveAt(i);
                            Console.WriteLine($"Protivnik {p.Ime} uništen u traci {traka.Broj}!");
                        }
                    }
                    else
                    {
                        int idx = trake.IndexOf(traka);
                        int idxNext = (idx + 1) % trake.Count;
                        TRAKA sledeca = trake[idxNext];

                        sledeca.strelacZona.Add(p);
                        traka.macevalacZona.RemoveAt(i);

                        Console.WriteLine($"{p.Ime} prešao iz trake {traka.Broj} u traku {sledeca.Broj} (strelac zona).");
                    }
                }
            }
        }

        private static void PremestiListuBezKatrana(List<PROTIVNIK> izListe, List<PROTIVNIK> uListu,
                                                  string izZone, string uZonu, int brojTrake)
        {
            List<PROTIVNIK> zaPremestanje = new List<PROTIVNIK>();

            foreach (PROTIVNIK protivnik in izListe)
            {
                if (protivniciZalepljeniKatranom.ContainsKey(protivnik) &&
                    protivniciZalepljeniKatranom[protivnik])
                {
                    protivniciZalepljeniKatranom[protivnik] = false;
                }
                else
                {
                    zaPremestanje.Add(protivnik);
                }
            }

            if (zaPremestanje.Count > 0)
            {
                Console.WriteLine($"Pomeranje {zaPremestanje.Count} protivnika iz {izZone} u {uZonu} zonu trake {brojTrake}");
                uListu.AddRange(zaPremestanje);

                foreach (PROTIVNIK p in zaPremestanje)
                    izListe.Remove(p);
            }
        }

        private static void ObradiSpecijalnogProtivnika(PROTIVNIK protivnik, TRAKA traka)
        {
            switch (protivnik.Ime)
            {
                case "Prelaze u sledeću traku":
                    foreach (TRAKA t in trake)
                    {
                        for (int i = t.macevalacZona.Count - 1; i >= 0; i--)
                        {
                            PROTIVNIK p = t.macevalacZona[i];
                            if (t.brojZidina == 0)
                            {
                                int sledecaTrakaIndex = (trake.IndexOf(t) + 1) % trake.Count;
                                trake[sledecaTrakaIndex].strelacZona.Add(p);
                                t.macevalacZona.RemoveAt(i);
                                Console.WriteLine($"{p.Ime} prešao iz trake {t.Broj} u traku {trake[sledecaTrakaIndex].Broj}");
                            }
                        }
                    }
                    break;

                case "Kuga! Strelci":
                    Console.WriteLine("Kuga! Strelci - karte strelaca se ne delje!");
                    dostupneKarte.RemoveAll(k => k.Naziv.Contains("Strelac"));
                    foreach (Socket klijent in karteIgraca.Keys)
                        karteIgraca[klijent].RemoveAll(k => k.Naziv.Contains("Strelac"));
                    break;

                case "Postavi još četiri protivnika":
                    Console.WriteLine("Dodajem još 4 protivnika!");
                    for (int i = 0; i < 4; i++)
                    {
                        if (preostaliProtivnici.Count > 0)
                        {
                            Random rand = new Random();
                            int idxProt = rand.Next(preostaliProtivnici.Count);
                            int idxTrake = rand.Next(trake.Count);
                            PROTIVNIK noviProt = preostaliProtivnici[idxProt];
                            preostaliProtivnici.RemoveAt(idxProt);
                            trake[idxTrake].suma.Add(noviProt);
                        }
                    }
                    break;

                case "Pomeranje plavih":
                    PomeriSveUTraciPoBoji(TRAKA.BOJA.plava);
                    break;
                case "Pomeranje zelenih":
                    PomeriSveUTraciPoBoji(TRAKA.BOJA.zelena);
                    break;
                case "Pomeranje crvenih":
                    PomeriSveUTraciPoBoji(TRAKA.BOJA.crvena);
                    break;
            }
        }

        private static void PomeriSveUTraciPoBoji(TRAKA.BOJA boja)
        {
            foreach (TRAKA traka in trake.Where(t => t.Boja == boja))
            {
                for (int i = traka.suma.Count - 1; i >= 0; i--)
                {
                    traka.strelacZona.Add(traka.suma[i]);
                    traka.suma.RemoveAt(i);
                }

                for (int i = traka.strelacZona.Count - 1; i >= 0; i--)
                {
                    traka.vitezZona.Add(traka.strelacZona[i]);
                    traka.strelacZona.RemoveAt(i);
                }

                for (int i = traka.vitezZona.Count - 1; i >= 0; i--)
                {
                    traka.macevalacZona.Add(traka.vitezZona[i]);
                    traka.vitezZona.RemoveAt(i);
                }

                Console.WriteLine($"Svi protivnici u {boja} trakama pomereni jednu zonu napred!");
            }
        }
    }
}
