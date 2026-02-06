using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using System.Threading;
using Biblioteka;


namespace CastleDefensePR.Client
{
    public partial class Client
    {
        private static void IzbaciKartu()
        {
            if (kartaIzbacenaUTrenutnomPotezu)
            {
                Console.WriteLine("Već ste izbacili kartu u ovom potezu!");
                return;
            }

            if (primljeneKarte == null || primljeneKarte.Count == 0)
            {
                Console.WriteLine("Nemate nijednu kartu za izbacivanje!");
                return;
            }

            Console.Write("Unesite broj karte koju želite da izbacite: ");
            string unos = Console.ReadLine();

            if (!int.TryParse(unos, out int broj))
            {
                Console.WriteLine("Neispravan unos! Unesite broj (npr. 1, 2, 3...).");
                return;
            }

            if (broj < 1 || broj > primljeneKarte.Count)
            {
                Console.WriteLine("Neispravan broj karte.");
                return;
            }

            KARTA karta = primljeneKarte[broj - 1];
            string poruka = $"IZBACUJEM {karta.Naziv}";

            try
            {
                SendText(tcpKlijent, poruka);

                kartaIzbacenaUTrenutnomPotezu = true;
                primljeneKarte.RemoveAt(broj - 1);

                Console.WriteLine($"Karta {karta.Naziv} je izbačena.");
                PrikaziKarte();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Greška pri slanju serveru: {ex.Message}");
                kartaIzbacenaUTrenutnomPotezu = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Neočekivana greška: {ex.Message}");
                kartaIzbacenaUTrenutnomPotezu = false;
            }
        }
    }
}
