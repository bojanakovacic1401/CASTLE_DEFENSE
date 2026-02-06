using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void DealInitialCards()
        {
            dostupneKarte = GameLogic.IzaberiKarte(brojIgraca);
            Random rnd = new Random();

            int brojKarataPoIgracu = (brojIgraca == 3) ? 4 : 5;
            int ukupnoPotrebnihKarata = brojKarataPoIgracu * brojIgraca;

            EnsureDeckHasEnoughCards(ukupnoPotrebnihKarata);

            Console.WriteLine($"\nDostupno karata: {dostupneKarte.Count}");
            Console.WriteLine($"Saljem po {brojKarataPoIgracu} karata igracima...\n");

            for (int i = 0; i < brojIgraca; i++)
            {
                List<KARTA> karteZaIgraca = new List<KARTA>();
                List<int> iskorisceneKarte = new List<int>();

                for (int j = 0; j < brojKarataPoIgracu; j++)
                {
                    int oznaka;
                    do
                    {
                        oznaka = rnd.Next(dostupneKarte.Count);
                    } while (iskorisceneKarte.Contains(oznaka));

                    iskorisceneKarte.Add(oznaka);
                    karteZaIgraca.Add(dostupneKarte[oznaka]);
                }

                foreach (int k in iskorisceneKarte.OrderByDescending(x => x))
                    dostupneKarte.RemoveAt(k);

                karteIgraca[tcpKlijenti[i]] = new List<KARTA>(karteZaIgraca);
                SendCards(tcpKlijenti[i], karteZaIgraca);

                Console.WriteLine($"Poslato {brojKarataPoIgracu} karata igracu {i + 1}");
                Console.WriteLine($"Karte za igraca {i + 1}:");
                foreach (KARTA karta in karteZaIgraca)
                    Console.WriteLine($"- {karta.Naziv}");
                Console.WriteLine();
            }
        }

        private static void EnsureDeckHasEnoughCards(int ukupnoPotrebnihKarata)
        {
            if (dostupneKarte.Count >= ukupnoPotrebnihKarata) return;

            Console.WriteLine($"\nUPOZORENJE: Nema dovoljno karata!");
            Console.WriteLine($"Dostupno: {dostupneKarte.Count}, Potrebno: {ukupnoPotrebnihKarata}");
            Console.WriteLine("Dupliramo dostupne karte...");

            List<KARTA> dodatneKarte = new List<KARTA>();
            while (dostupneKarte.Count + dodatneKarte.Count < ukupnoPotrebnihKarata)
                dodatneKarte.AddRange(GameLogic.IzaberiKarte(brojIgraca));

            dostupneKarte.AddRange(dodatneKarte);
        }
    }
}
