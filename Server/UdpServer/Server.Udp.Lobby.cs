using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
private static void ReadBrojIgraca()
        {
            while (true)
            {
                Console.WriteLine("Unesite broj igraca (1-3):");
                string unos = Console.ReadLine();

                if (!int.TryParse(unos, out brojIgraca))
                {
                    Console.WriteLine("Unos mora biti broj, pokusajte ponovo!");
                    continue;
                }
                if (brojIgraca >= 1 && brojIgraca <= 3)
                    break;

                Console.WriteLine("Broj igraca mora biti izmedju 1 i 3! Pokusajte ponovo.");
            }
        }
private static List<EndPoint> LobbyUdpCollect()
        {
            Socket udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint udpEndPoint = new IPEndPoint(IPAddress.Any, 5555);
            udpServer.Bind(udpEndPoint);

            Console.WriteLine($"UDP server pokrenut na {udpEndPoint}. Cekam {brojIgraca} prijava...");

            List<EndPoint> prijavljeniIgraci = new List<EndPoint>();
            byte[] buffer = new byte[1024];

            while (prijavljeniIgraci.Count < brojIgraca)
            {
                EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                int bytesReceived = udpServer.ReceiveFrom(buffer, ref clientEndPoint);
                string poruka = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

                if (poruka == "PRIJAVA")
                {
                    prijavljeniIgraci.Add(clientEndPoint);
                    Console.WriteLine($"Prijava primljena od {clientEndPoint}");
                }
            }

            udpServer.Close();
            return prijavljeniIgraci;
        }
private static void SendTcpAddressToClients(List<EndPoint> prijavljeniIgraci, string serverIP, int tcpPort)
        {
            Socket udpZaSlanje = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            string tcpAdresa = $"{serverIP}:{tcpPort}";
            byte[] porukaZaKlijente = Encoding.UTF8.GetBytes(tcpAdresa);

            foreach (EndPoint ep in prijavljeniIgraci)
            {
                udpZaSlanje.SendTo(porukaZaKlijente, ep);
                Console.WriteLine($"Poslata TCP adresa {tcpAdresa} igraču {ep}");
            }

            udpZaSlanje.Close();
        }
    }
}
