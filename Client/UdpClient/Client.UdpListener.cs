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
        private static bool IsOdbijeno(byte[] buf, int len)
        {
            if (len != 8) return false; // "ODBIJENO" = 8 bajtova
            return buf[0] == (byte)'O' &&
                   buf[1] == (byte)'D' &&
                   buf[2] == (byte)'B' &&
                   buf[3] == (byte)'I' &&
                   buf[4] == (byte)'J' &&
                   buf[5] == (byte)'E' &&
                   buf[6] == (byte)'N' &&
                   buf[7] == (byte)'O';
        }

        private static void OsluskujUdp()
        {
            // Listener samo za PRIJEM zahteva (karta kao objekat)
            udpRazmena.ReceiveTimeout = 500; // da nit može da se ugasi

            byte[] buffer = new byte[8192];
            EndPoint remote = new IPEndPoint(IPAddress.Any, 0);

#pragma warning disable SYSLIB0011
            BinaryFormatter bf = new BinaryFormatter();
#pragma warning restore SYSLIB0011

            while (udpAktivan)
            {
                try
                {
                    int n = udpRazmena.ReceiveFrom(buffer, ref remote);

                    // Ovde očekujemo objekat KARTA (zahtev za zamenu)
                    KARTA karta;
                    using (var ms = new MemoryStream(buffer, 0, n))
                    {
#pragma warning disable SYSLIB0011
                        karta = (KARTA)bf.Deserialize(ms);
#pragma warning restore SYSLIB0011
                    }

                    lock (lockObject)
                    {
                        primljenaKartaZaZamenu = karta;
                        udpKlijentZaZamenu = remote;
                        zahtevZaZamenu = true;
                    }

                    Console.WriteLine($"\n[UDP] Stigao zahtev za zamenu: {karta?.Naziv}");
                    Console.WriteLine("U svom potezu izaberi opciju za obradu zamene (ili kad vidiš meni).");
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    // normalno (timeout da proverimo udpAktivan)
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UDP] Greška: {ex.Message}");
                }
            }
        }
    }
}
