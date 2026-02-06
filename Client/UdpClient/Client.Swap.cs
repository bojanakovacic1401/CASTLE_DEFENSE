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
        private static void ObradiZamenuUGlavnojNiti()
        {
            KARTA kartaPonude;
            EndPoint ep;

            lock (lockObject)
            {
                if (!zahtevZaZamenu || primljenaKartaZaZamenu == null || udpKlijentZaZamenu == null)
                {
                    Console.WriteLine("Nema aktivnog zahteva za zamenu.");
                    return;
                }

                kartaPonude = primljenaKartaZaZamenu;
                ep = udpKlijentZaZamenu;

                // reset flag odmah da ne dupliraš
                zahtevZaZamenu = false;
                primljenaKartaZaZamenu = null;
                udpKlijentZaZamenu = null;
            }

            // Ako si već radila zamenu u ovom potezu, automatski odbij
            if (zamenaUradjena)
            {
                udpRazmena.SendTo(Encoding.UTF8.GetBytes("ODBIJENO"), ep);
                Console.WriteLine("Zamena odbijena (već si radila zamenu u ovom potezu).");
                return;
            }

            // Traži od servera dozvolu (jedna zamena po potezu)
            zamenaDozvoljena = false;
            zamenaProveraEvent.Reset();
            SendText(tcpKlijent, "ZAMENA");

            if (!zamenaProveraEvent.Wait(2000) || !zamenaDozvoljena)
            {
                udpRazmena.SendTo(Encoding.UTF8.GetBytes("ODBIJENO"), ep);
                Console.WriteLine("Zamena odbijena (server: zamena nije dozvoljena u ovom potezu).");
                return;
            }

            Console.WriteLine($"\nStigao zahtev za zamenu. Protivnik nudi: {kartaPonude.Naziv}");
            Console.Write("Da li prihvataš zamenu? (da/ne): ");
            string odgovor = Console.ReadLine()?.Trim().ToLower();

            if (odgovor != "da")
            {
                udpRazmena.SendTo(Encoding.UTF8.GetBytes("ODBIJENO"), ep);
                Console.WriteLine("Zamena odbijena.");
                return;
            }

            if (primljeneKarte.Count == 0)
            {
                udpRazmena.SendTo(Encoding.UTF8.GetBytes("ODBIJENO"), ep);
                Console.WriteLine("Nemaš karte za zamenu.");
                return;
            }

            PrikaziKarte();
            Console.Write("Izaberi broj karte koju šalješ: ");
            if (!int.TryParse(Console.ReadLine(), out int broj) || broj < 1 || broj > primljeneKarte.Count)
            {
                udpRazmena.SendTo(Encoding.UTF8.GetBytes("ODBIJENO"), ep);
                Console.WriteLine("Neispravan izbor, zamena odbijena.");
                return;
            }

            KARTA kartaZaSlanje = primljeneKarte[broj - 1];

#pragma warning disable SYSLIB0011
            BinaryFormatter bf = new BinaryFormatter();
#pragma warning restore SYSLIB0011

            // Pošalji kartu nazad preko UDP
            using (var ms = new MemoryStream())
            {
#pragma warning disable SYSLIB0011
                bf.Serialize(ms, kartaZaSlanje);
#pragma warning restore SYSLIB0011
                byte[] data = ms.ToArray();
                udpRazmena.SendTo(data, ep);
            }

            // Ažuriraj lokalnu ruku
            primljeneKarte.RemoveAt(broj - 1);
            primljeneKarte.Add(kartaPonude);

            zamenaUradjena = true;

            // Sync serveru (autoritet)
            PosaljiZamenaSync(kartaZaSlanje, kartaPonude);

            Console.WriteLine($"Zamena uspešna! Dao/la: {kartaZaSlanje.Naziv}, dobio/la: {kartaPonude.Naziv}");
            PrikaziKarte();
        }

        private static void ZameniKartu()
        {
            if (zamenaUradjena)
            {
                Console.WriteLine("Već si radila zamenu u ovom potezu.");
                return;
            }

            // Traži dozvolu od servera (1 zamena po potezu)
            zamenaDozvoljena = false;
            zamenaProveraEvent.Reset();
            SendText(tcpKlijent, "ZAMENA");

            if (!zamenaProveraEvent.Wait(2000) || !zamenaDozvoljena)
            {
                Console.WriteLine("Zamena nije dozvoljena u ovom potezu.");
                return;
            }

            if (primljeneKarte.Count == 0)
            {
                Console.WriteLine("Nemaš karte za zamenu.");
                return;
            }

            PrikaziUdpInformacije();
            PrikaziKarte();

            Console.Write("Izaberi broj karte koju šalješ: ");
            if (!int.TryParse(Console.ReadLine(), out int broj) || broj < 1 || broj > primljeneKarte.Count)
            {
                Console.WriteLine("Neispravan izbor.");
                return;
            }

            KARTA kartaZaSlanje = primljeneKarte[broj - 1];

            // unos IP/porta sa retry
            IPEndPoint ciljEP = null;
            while (ciljEP == null)
            {
                Console.Write("Unesi IP drugog igrača: ");
                string ipStr = Console.ReadLine()?.Trim();

                Console.Write("Unesi UDP port drugog igrača: ");
                string portStr = Console.ReadLine()?.Trim();

                if (!IPAddress.TryParse(ipStr, out var ip) || !int.TryParse(portStr, out int port))
                {
                    Console.WriteLine("Neispravan IP/port. Pokušaj opet.");
                    continue;
                }

                ciljEP = new IPEndPoint(ip, port);
            }

            // PRIVREMENI UDP soket za ovu zamenu (izbegava konflikt sa OsluskujUdp)
            using (Socket udpSwap = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                udpSwap.Bind(new IPEndPoint(IPAddress.Any, 0));
                udpSwap.ReceiveTimeout = 10000;

#pragma warning disable SYSLIB0011
                BinaryFormatter bf = new BinaryFormatter();
#pragma warning restore SYSLIB0011

                // pošalji kartu
                byte[] sendData;
                using (var ms = new MemoryStream())
                {
#pragma warning disable SYSLIB0011
                    bf.Serialize(ms, kartaZaSlanje);
#pragma warning restore SYSLIB0011
                    sendData = ms.ToArray();
                }

                udpSwap.SendTo(sendData, ciljEP);

                // primi odgovor
                byte[] buf = new byte[8192];
                EndPoint respEP = new IPEndPoint(IPAddress.Any, 0);

                try
                {
                    int n = udpSwap.ReceiveFrom(buf, ref respEP);

                    if (IsOdbijeno(buf, n))
                    {
                        Console.WriteLine("Drugi igrač je odbio zamenu.");
                        return;
                    }

                    KARTA primljena;
                    using (var ms2 = new MemoryStream(buf, 0, n))
                    {
#pragma warning disable SYSLIB0011
                        primljena = (KARTA)bf.Deserialize(ms2);
#pragma warning restore SYSLIB0011
                    }

                    // update ruke
                    primljeneKarte.RemoveAt(broj - 1);
                    primljeneKarte.Add(primljena);

                    zamenaUradjena = true;

                    // sync serveru
                    PosaljiZamenaSync(kartaZaSlanje, primljena);

                    Console.WriteLine($"Zamena uspešna! Dobio/la: {primljena.Naziv}");
                    PrikaziKarte();
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    Console.WriteLine("Nije stigao odgovor (timeout). Proveri IP/port i pokušaj ponovo.");
                }
            }
        }
    }
}
