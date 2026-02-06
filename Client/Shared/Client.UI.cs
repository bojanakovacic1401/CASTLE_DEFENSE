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
        private static void PrikaziUdpInformacije()
        {
            if (udpRazmena == null || udpRazmena.LocalEndPoint == null)
                return;

            IPEndPoint ep = (IPEndPoint)udpRazmena.LocalEndPoint;

            Console.WriteLine();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("UDP PODACI ZA ZAMENU KARATA");
            Console.WriteLine($"IP adresa : {ep.Address}");
            Console.WriteLine($"Port      : {ep.Port}");
            Console.WriteLine("Drugi igrači se OVDE javljaju za zamenu karata.");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine();
        }

        private static void PrikaziKarte()
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("VAŠE KARTE:");
            Console.WriteLine(new string('-', 50));

            int brojKarte = 1;
            foreach (var karta in primljeneKarte)
            {
                Console.WriteLine($"{brojKarte++}. {karta.Naziv}");
                Console.WriteLine($"   Efekat: {karta.Efekat}");
                Console.WriteLine();
            }
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Ukupno karata: {primljeneKarte.Count} / {maxBrojKarata}\n");
        }
    }
}
