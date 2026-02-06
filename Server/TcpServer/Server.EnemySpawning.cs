using System;
using System.Linq;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void PostaviNoveProtivnike()
        {
            if (preostaliProtivnici.Count == 0)
            {
                Console.WriteLine("\nNema više protivnika za postavljanje!");
                return;
            }

            Console.WriteLine($"\n=== POSTAVLJANJE NOVIH PROTIVNIKA (preostalo: {preostaliProtivnici.Count}) ===");
            Random rnd = new Random();
            int brojZaPostavljanje = Math.Min(2, preostaliProtivnici.Count);

            for (int i = 0; i < brojZaPostavljanje; i++)
            {
                int idxProt = rnd.Next(preostaliProtivnici.Count);
                int idxTrake = rnd.Next(trake.Count);

                PROTIVNIK p = preostaliProtivnici[idxProt];
                preostaliProtivnici.RemoveAt(idxProt);

                trake[idxTrake].suma.Add(p);
                Console.WriteLine($"Novi protivnik {p.Ime} (HP: {p.ZivotniPoeni}) postavljen u Šumu trake {trake[idxTrake].Broj}");
            }
        }
    }
}
