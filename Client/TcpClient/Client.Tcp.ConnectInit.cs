using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Biblioteka;

namespace CastleDefensePR.Client
{
    public partial class Client
    {
        /// <summary>
        /// TCP konekcija na server i prijem početnih karata (MSG_CARDS).
        /// </summary>
        private static void TcpPoveziSeIPrimIgrackeKarte(string tcpIP, int tcpPort)
        {
            tcpKlijent = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine($"Povezujem se na TCP server {tcpIP}:{tcpPort}...");
            tcpKlijent.Connect(new IPEndPoint(IPAddress.Parse(tcpIP), tcpPort));
            Console.WriteLine("Uspostavljena TCP veza sa serverom!");

            // očekujemo da prva poruka bude lista karata
            var (type0, payload0) = ReceiveFrame(tcpKlijent);
            if (type0 != MSG_CARDS)
                throw new InvalidOperationException("Neočekivana poruka umesto početnih karata.");

#pragma warning disable SYSLIB0011
            formatter = new BinaryFormatter();
#pragma warning restore SYSLIB0011

            using (MemoryStream ms = new MemoryStream(payload0))
            {
#pragma warning disable SYSLIB0011
                primljeneKarte = (List<KARTA>)formatter.Deserialize(ms);
#pragma warning restore SYSLIB0011
            }

            maxBrojKarata = primljeneKarte.Count;
            Console.WriteLine($"Maksimalan broj karata u ruci: {maxBrojKarata}");
            PrikaziKarte();
        }
    }
}
