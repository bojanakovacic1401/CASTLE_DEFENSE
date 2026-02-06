using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Biblioteka;

namespace CastleDefensePR.Client
{
    public partial class Client
    {
        /// <summary>
        /// UDP "PRIJAVA" prema serveru (port 5555). Server vraća tekst "ip:port" za TCP.
        /// </summary>
        private static (string tcpIP, int tcpPort) UdpPrijavaISaznajTcpAdresu(string serverIP, int udpPort)
        {
            using Socket udpKlijent = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), udpPort);

            // šalji PRIJAVA
            byte[] prijavaData = Encoding.UTF8.GetBytes("PRIJAVA");
            udpKlijent.SendTo(prijavaData, serverEndPoint);

            Console.WriteLine($"Prijava poslata na {serverEndPoint}. Čekam odgovor...");

            byte[] buffer = new byte[1024];
            EndPoint tempEP = new IPEndPoint(IPAddress.Any, 0);
            udpKlijent.ReceiveTimeout = 5000;

            int bytesReceived = udpKlijent.ReceiveFrom(buffer, ref tempEP);
            string tcpAdresa = Encoding.UTF8.GetString(buffer, 0, bytesReceived);

            Console.WriteLine($"Primljena TCP adresa: {tcpAdresa}");

            string[] delovi = tcpAdresa.Split(':');
            if (delovi.Length != 2)
                throw new InvalidOperationException($"Neispravna TCP adresa: {tcpAdresa}");

            string tcpIP = delovi[0];
            int tcpPort = int.Parse(delovi[1]);

            return (tcpIP, tcpPort);
        }

        /// <summary>
        /// UDP soket za razmenu karata (peer-to-peer). Bind na slučajan port.
        /// </summary>
        private static void UdpPokreniRazmenu()
        {
            udpRazmena = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpRazmena.Bind(new IPEndPoint(IPAddress.Any, 0));
        }
    }
}
