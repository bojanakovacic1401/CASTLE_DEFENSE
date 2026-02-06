using System;
using System.Collections.Generic;
using System.Linq;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static List<PROTIVNIK> IzvuciProtivnikeZaPocetak(List<PROTIVNIK> sviProtivnici, int brojIgraca)
        {
            List<PROTIVNIK> rezultat = new List<PROTIVNIK>();
            Dictionary<string, int> potrebno = new Dictionary<string, int>();

            if (brojIgraca == 1)
            {
                potrebno.Add("Ork", 1);
                potrebno.Add("Trol", 1);
            }
            else if (brojIgraca == 2)
            {
                potrebno.Add("Goblin", 1);
                potrebno.Add("Ork", 2);
                potrebno.Add("Trol", 1);
            }
            else if (brojIgraca == 3)
            {
                potrebno.Add("Goblin", 2);
                potrebno.Add("Ork", 3);
                potrebno.Add("Trol", 1);
            }

            foreach (var pot in potrebno)
            {
                for (int i = 0; i < pot.Value; i++)
                {
                    PROTIVNIK prot = sviProtivnici.FirstOrDefault(p => p.Ime == pot.Key);
                    if (prot != null)
                    {
                        rezultat.Add(prot);
                        sviProtivnici.Remove(prot);
                    }
                }
            }
            return rezultat;
        }

        private static void SetupInitialEnemies()
        {
            List<PROTIVNIK> sviProtivnici = GameLogic.IzaberiProtivnike(brojIgraca);
            List<PROTIVNIK> protivniciZaPostavljanje = IzvuciProtivnikeZaPocetak(sviProtivnici, brojIgraca);
            preostaliProtivnici = sviProtivnici;

            Random rand = new Random();
            List<int> indeksiTraka = Enumerable.Range(0, trake.Count).ToList();

            for (int i = indeksiTraka.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                int temp = indeksiTraka[i];
                indeksiTraka[i] = indeksiTraka[j];
                indeksiTraka[j] = temp;
            }

            for (int i = 0; i < protivniciZaPostavljanje.Count && i < indeksiTraka.Count; i++)
            {
                int indeksTrake = indeksiTraka[i];
                trake[indeksTrake].strelacZona.Add(protivniciZaPostavljanje[i]);
                Console.WriteLine($"Postavljen {protivniciZaPostavljanje[i].Ime} (HP: {protivniciZaPostavljanje[i].ZivotniPoeni}) u traku {trake[indeksTrake].Broj}");
            }

            Console.WriteLine("\n----- STANJE TRAKA -----");
            foreach (TRAKA traka in trake)
            {
                Console.WriteLine($"\nTraka {traka.Broj} ({traka.Boja}):");
                Console.WriteLine($"Zidovi: {traka.brojZidina}");

                if (traka.strelacZona.Count > 0)
                {
                    Console.WriteLine("\nStrelac Zona:");
                    foreach (PROTIVNIK protivnik in traka.strelacZona)
                        Console.WriteLine($"  - {protivnik.Ime} - HP: {protivnik.ZivotniPoeni}");
                }
                else
                {
                    Console.WriteLine("\nStrelac Zona: prazna");
                }
            }
        }
    }
}
