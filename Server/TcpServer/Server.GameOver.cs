using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void ProveriKrajPoteza(List<Socket> aktivniKlijenti)
        {
            bool sviZavrsili = true;
            foreach (var soket in aktivniKlijenti)
            {
                if (potezZavrsen.ContainsKey(soket) && !potezZavrsen[soket])
                {
                    sviZavrsili = false;
                    break;
                }
            }
            if (sviZavrsili && aktivniKlijenti.Count > 0)
            {
                Console.WriteLine("\n=== SVI IGRACI SU ZAVRŠILI POTEZ (nakon diskonekcije) ===");
                Console.WriteLine("Počinje novi potez...\n");
            }
        }

        private static bool ProveriKrajIgre()
        {
            int ukupnoZidina = trake.Sum(t => t.brojZidina);
            if (ukupnoZidina == 0)
            {
                Console.WriteLine("\n=== IGRAČI SU IZGUBILI! Svi zidovi su uništeni. ===");
                return true;
            }

            int ukupnoProtivnika = trake.Sum(t => t.suma.Count + t.strelacZona.Count +
                                                t.vitezZona.Count + t.macevalacZona.Count);
            if (ukupnoProtivnika == 0 && preostaliProtivnici.Count == 0)
            {
                Console.WriteLine("\n=== IGRAČI SU POBEDILI! Svi protivnici su uništeni. ===");
                return true;
            }

            return false;
        }

        private static void PosaljiPorukuOKrajuIgre(List<Socket> aktivniKlijenti)
        {
            string poruka;
            int ukupnoZidina = trake.Sum(t => t.brojZidina);

            poruka = (ukupnoZidina == 0) ? "GAME_OVER_IZGUBILI_STE" : "GAME_OVER_POBEDILI_STE";

            foreach (var soket in aktivniKlijenti)
            {
                try { SendText(soket, poruka); }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška pri slanju poruke o kraju igre: {ex.Message}");
                }
            }
        }
    }
}
