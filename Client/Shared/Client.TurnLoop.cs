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
        private static void IzvrsiPotez()
        {
            if (!igraTraje) return;

            lock (lockObject)
            {
                if (zahtevZaZamenu)
                {
                    ObradiZamenuUGlavnojNiti();
                    return;
                }
            }

            if (!potezUToku)
            {
                potezUToku = true;
                kartuOdigraoUTrenutnomPotezu = false;
                zamenaUradjena = false;
                PrikaziUdpInformacije();
                Console.WriteLine("\n=== NOVI POTEZ ===");
            }

            bool potezZavršen = false;

            while (!potezZavršen && igraTraje)
            {
                Console.WriteLine("\nOpcije poteza:");
                Console.WriteLine("1. Izbaci kartu");
                Console.WriteLine("2. Zameni kartu sa drugim igračem");
                Console.WriteLine(kartuOdigraoUTrenutnomPotezu ?
                    "3. Odigraj kartu (VEĆ STE ODIGRALI KARTU U OVOM POTEZU)" :
                    "3. Odigraj kartu");
                Console.WriteLine("4. Završi potez");
                Console.Write("Izbor: ");

                string opcija = Console.ReadLine();

                switch (opcija)
                {
                    case "1":
                        IzbaciKartu();
                        break;
                    case "2":
                        ZameniKartu();
                        break;
                    case "3":
                        if (!kartuOdigraoUTrenutnomPotezu)
                            OdigrajKartu();
                        else
                            Console.WriteLine("Već ste odigrali kartu u ovom potezu!");
                        break;
                    case "4":
                        ZavrsiPotez();
                        potezZavršen = true;
                        break;
                    default:
                        Console.WriteLine("Nepoznata opcija, pokušajte ponovo.");
                        break;
                }

                // Proveravamo da li je stigao zahtev za zamenu
                lock (lockObject)
                {
                    if (zahtevZaZamenu)
                    {
                        ObradiZamenuUGlavnojNiti();
                    }
                }
            }
        }

        private static void ZavrsiPotez()
        {
            int nedostaje = Math.Max(0, maxBrojKarata - primljeneKarte.Count);
            string poruka = $"ZAVRSIO_POTEZ {nedostaje}";
            SendText(tcpKlijent, poruka);

            Console.WriteLine("\n=== POTEZ ZAVRŠEN ===");
            Console.WriteLine($"Tražite {nedostaje} novih karata.");
            Console.WriteLine("Čekam druge igrače i server...");

            potezAktivan = false;
            potezUToku = false;
        }
    }
}
