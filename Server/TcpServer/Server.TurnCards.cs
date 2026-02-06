using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Biblioteka;

using System.Net.Sockets;
namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void DopuniKarteIgracima(List<Socket> aktivniKlijenti)
        {
            if (dostupneKarte == null || dostupneKarte.Count == 0)
            {
                Console.WriteLine("Nema više karata u špilu!");
                return;
            }

            Random rnd = new Random();

            foreach (var soket in aktivniKlijenti)
            {
                if (!trazeneKarte.ContainsKey(soket) || !karteIgraca.ContainsKey(soket))
                    continue;

                int koliko = trazeneKarte[soket];
                if (koliko <= 0) continue;

                List<KARTA> noveKarte = new List<KARTA>();

                for (int i = 0; i < koliko && dostupneKarte.Count > 0; i++)
                {
                    int idx = rnd.Next(dostupneKarte.Count);
                    noveKarte.Add(dostupneKarte[idx]);
                    dostupneKarte.RemoveAt(idx);
                }

                if (noveKarte.Count > 0)
                {
                    karteIgraca[soket].AddRange(noveKarte);
                    SendCards(soket, noveKarte);
                }
            }
        }
    }
}
