using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        static void Main(string[] args)
        {
            ReadBrojIgraca();

            // 1) UDP lobby prijave
            List<EndPoint> prijavljeniIgraci = LobbyUdpCollect();

            // 2) TCP server + slanje TCP adrese UDP-om
            Socket tcpServer = SetupTcpServer(out string serverIP);
            SendTcpAddressToClients(prijavljeniIgraci, serverIP, 6666);

            // 3) Prihvati klijente + init state
            AcceptAllClients(tcpServer);
            InitClientState();

            // 4) Kreiraj trake
            SetupTracks();

            // 5) Karte: špil + slanje početnih karata
            DealInitialCards();

            // 6) Protivnici: početno postavljanje
            SetupInitialEnemies();

            Console.WriteLine("\n=== POČINJE IGRA ===");
            Console.WriteLine("Server prelazi u neblokirajući režim rada...");

            // 7) Game loop
            RunGameLoop(tcpServer);

            tcpServer.Close();
            Console.WriteLine("\nServer zavrsio rad. Pritisnite Enter...");
            Console.ReadLine();
        }
    }
}
