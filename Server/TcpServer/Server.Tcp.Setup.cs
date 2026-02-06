using System;
using System.Net;
using System.Net.Sockets;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
private static Socket SetupTcpServer(out string serverIP)
        {
            Socket tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Any, 6666);
            tcpServer.Bind(tcpEndPoint);
            tcpServer.Listen(brojIgraca);

            serverIP = GetLocalIPAddress();
            Console.WriteLine($"Server IP adresa: {serverIP}");
            return tcpServer;
        }
private static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            throw new Exception("No network adapters with an IPv4 address!");
        }
private static void AcceptAllClients(Socket tcpServer)
        {
            tcpKlijenti.Clear();
            for (int i = 0; i < brojIgraca; i++)
            {
                Socket klijent = tcpServer.Accept();
                klijent.Blocking = false;
                tcpKlijenti.Add(klijent);
                Console.WriteLine($"TCP konekcija uspostavljena sa klijentom {i + 1}");
            }
        }
private static void InitClientState()
        {
            foreach (Socket klijent in tcpKlijenti)
            {
                potezZavrsen[klijent] = false;
                kartuOdigrao[klijent] = false;
                kartaIzbacena[klijent] = false;
                trazeneKarte[klijent] = 0;
                karteIgraca[klijent] = new List<KARTA>();
            }
        }
    }
}
