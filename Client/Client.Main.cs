using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Biblioteka;

namespace CastleDefensePR.Client
{
    public partial class Client
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("=== CASTLE DEFENSE ===");
                Console.WriteLine("Unesite IP adresu servera (za lokalno testiranje upišite 127.0.0.1):");
                string serverIP = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(serverIP))
                    serverIP = "127.0.0.1";

                // 1) UDP prijava -> dobijanje TCP adrese servera
                var (tcpIP, tcpPort) = UdpPrijavaISaznajTcpAdresu(serverIP, 5555);

                // 2) TCP konekcija + prijem početnih karata
                TcpPoveziSeIPrimIgrackeKarte(tcpIP, tcpPort);

                // 3) UDP soket za razmenu karata (peer-to-peer)
                UdpPokreniRazmenu();

                // 4) Pokretanje niti (UDP listener + TCP listener)
                Thread udpThread = new Thread(OsluskujUdp) { IsBackground = true };
                udpThread.Start();

                serverThread = new Thread(OsluskujServer) { IsBackground = true };
                serverThread.Start();

                // 5) Glavna petlja poteza
                while (igraTraje)
                {
                    if (potezAktivan)
                        IzvrsiPotez();
                    else
                        Thread.Sleep(100);
                }

                // 6) Gašenje
                potezAktivan = false;
                udpAktivan = false;
                igraTraje = false;

                if (udpThread.IsAlive) udpThread.Join(1000);
                if (serverThread.IsAlive) serverThread.Join(1000);

                try { udpRazmena?.Close(); } catch { }

                try
                {
                    tcpKlijent?.Shutdown(SocketShutdown.Both);
                }
                catch { }
                try { tcpKlijent?.Close(); } catch { }

                Console.WriteLine("\nKonekcija zatvorena. Igra je završena!");
            }
            catch (FormatException)
            {
                Console.WriteLine("Neispravna IP adresa!");
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Socket greška: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Neočekivana greška: " + ex.Message);
            }

            Console.WriteLine("\nPritisnite Enter za izlaz...");
            Console.ReadLine();
        }
    }
}
