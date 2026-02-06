using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void RunGameLoop(Socket tcpServer)
        {
            List<Socket> aktivniKlijenti = new List<Socket>(tcpKlijenti);

            bool serverRadi = true;
            while (serverRadi)
            {
                if (aktivniKlijenti.Count == 0)
                {
                    Console.WriteLine("Nema aktivnih klijenata. Server se gasi.");
                    serverRadi = false;
                    break;
                }

                List<Socket> zaCitanje = new List<Socket>(aktivniKlijenti);
                List<Socket> zaGreske = new List<Socket>(aktivniKlijenti);

                try
                {
                    Socket.Select(zaCitanje, null, zaGreske, 1000000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška u Socket.Select: {ex.Message}");
                    continue;
                }

                foreach (Socket soket in zaGreske)
                {
                    Console.WriteLine($"Greška na soketu {soket.RemoteEndPoint}");
                    DisconnectClient(aktivniKlijenti, soket);
                    ProveriKrajPoteza(aktivniKlijenti);
                }

                foreach (Socket soket in zaCitanje)
                {
                    if (!aktivniKlijenti.Contains(soket))
                        continue;

                    try
                    {
                        byte[] temp = new byte[4096];
                        int primljenoBajtova = soket.Receive(temp);

                        if (primljenoBajtova == 0)
                            throw new SocketException();

                        var buf = GetRecvBuffer(soket);
                        for (int i = 0; i < primljenoBajtova; i++)
                            buf.Add(temp[i]);

                        while (TryParseFrame(buf, out byte type, out byte[] payload))
                        {
                            if (type == MSG_TEXT)
                                ObradiPoruku(Encoding.UTF8.GetString(payload), soket);
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.WouldBlock)
                            continue;

                        DisconnectClient(aktivniKlijenti, soket);
                        ProveriKrajPoteza(aktivniKlijenti);
                    }
                    catch (Exception)
                    {
                        DisconnectClient(aktivniKlijenti, soket);
                        ProveriKrajPoteza(aktivniKlijenti);
                    }
                }

                if (aktivniKlijenti.Count > 0 && aktivniKlijenti.All(s => potezZavrsen.ContainsKey(s) && potezZavrsen[s]))
                {
                    Console.WriteLine("\n=== KRAJ POTEZA ===");

                    PrikaziNetstatInformacije();
                    DopuniKarteIgracima(aktivniKlijenti);
                    PomeriProtivnike();
                    PostaviNoveProtivnike();
                    IspisiStanjeTraka();

                    ResetTurnState(aktivniKlijenti);
                    SendNewTurnSignal(aktivniKlijenti);

                    if (ProveriKrajIgre())
                    {
                        serverRadi = false;
                        PosaljiPorukuOKrajuIgre(aktivniKlijenti);
                        break;
                    }

                    Console.WriteLine("Počinje novi potez...\n");
                    continue;
                }

                Thread.Sleep(10);
            }
        }

        private static void DisconnectClient(List<Socket> aktivniKlijenti, Socket soket)
        {
            try { soket.Close(); } catch { }

            aktivniKlijenti.Remove(soket);
            potezZavrsen.Remove(soket);
            kartuOdigrao.Remove(soket);
            trazeneKarte.Remove(soket);
            karteIgraca.Remove(soket);
            kartaIzbacena.Remove(soket);
            RemoveClientState(soket);
            tcpKlijenti.Remove(soket);

            if (zamenaOd == soket || zamenaKa == soket)
            {
                zamenaUToku = false;
                zamenaOd = null;
                zamenaKa = null;
                kartaZaZamenu = null;
            }
        }

        private static void ResetTurnState(List<Socket> aktivniKlijenti)
        {
            zamenaSyncDone.Clear();
            zamenaVecIskoriscenaUTomPotezu = false;
            zamenaUToku = false;
            zamenaOd = null;
            zamenaKa = null;
            kartaZaZamenu = null;

            foreach (var soket in aktivniKlijenti)
            {
                potezZavrsen[soket] = false;
                kartuOdigrao[soket] = false;
                trazeneKarte[soket] = 0;
                kartaIzbacena[soket] = false;
            }
        }

        private static void SendNewTurnSignal(List<Socket> aktivniKlijenti)
        {
            foreach (var soket in aktivniKlijenti)
            {
                try { SendText(soket, "NOVI_POTEZ"); }
                catch (Exception ex)
                {
                    Console.WriteLine($"Greška pri slanju signala za novi potez: {ex.Message}");
                }
            }
        }
    }
}
