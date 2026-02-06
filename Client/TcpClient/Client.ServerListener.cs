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
        private static void OsluskujServer()
        {
#pragma warning disable SYSLIB0011
            BinaryFormatter formatterLocal = new BinaryFormatter();
#pragma warning restore SYSLIB0011

            while (igraTraje)
            {
                try
                {
                    // Poll u mikrosekundama (npr. 200ms)
                    bool spremno = tcpKlijent.Poll(200_000, SelectMode.SelectRead);
                    if (!spremno)
                        continue;

                    // Ako je spremno za čitanje a nema podataka -> konekcija zatvorena
                    if (tcpKlijent.Available == 0)
                    {
                        Console.WriteLine("Server je zatvorio konekciju.");
                        igraTraje = false;
                        potezAktivan = false;
                        break;
                    }

                    // Čitamo TAČNO jednu poruku (frame)
                    var (type, payload) = ReceiveFrame(tcpKlijent);

                    if (type == MSG_TEXT)
                    {
                        string tekstPoruka = Encoding.UTF8.GetString(payload);

                        if (tekstPoruka.StartsWith("GAME_OVER"))
                        {
                            if (tekstPoruka.Contains("POBEDILI"))
                                Console.WriteLine("Pobedili ste!");
                            else if (tekstPoruka.Contains("IZGUBILI"))
                                Console.WriteLine("Izgubili ste! Uništene su sve zidine.");
                            else
                                Console.WriteLine("Kraj igre.");

                            potezAktivan = false;
                            return;
                        }
                        else if (tekstPoruka.StartsWith("ZAMENA_DOZVOLJENA"))
                        {
                            zamenaDozvoljena = true;
                            zamenaProveraEvent.Set();
                        }
                        else if (tekstPoruka.StartsWith("ZAMENA_ODBIJENA"))
                        {
                            zamenaDozvoljena = false;
                            Console.WriteLine(tekstPoruka);
                            zamenaProveraEvent.Set();
                        }
                        else if (tekstPoruka.StartsWith("ZAMENA_SYNC_OK"))
                        {
                            Console.WriteLine("Server: zamena uspešno sinhronizovana.");
                        }
                        else if (tekstPoruka.StartsWith("ZAMENA_SYNC_FAIL"))
                        {
                            Console.WriteLine($"Server nije sinhronizovao zamenu: {tekstPoruka}");
                        }
                        else if (tekstPoruka.StartsWith("NOVI_POTEZ"))
                        {
                            potezAktivan = true;
                            potezUToku = false;
                            kartuOdigraoUTrenutnomPotezu = false;
                            zamenaUradjena = false;
                            kartaIzbacenaUTrenutnomPotezu = false;

                            Console.WriteLine("\nNOVI POTEZ JE POČEO");
                            PrikaziUdpInformacije();

                            try { SendText(tcpKlijent, "NOVI_POTEZ_POTVRDA"); } catch { }
                        }
                        else
                        {
                            Console.WriteLine("\n[Server] " + tekstPoruka);
                        }
                    }
                    else if (type == MSG_CARDS)
                    {
                        using (MemoryStream ms = new MemoryStream(payload))
                        {
                            List<KARTA> noveKarte = (List<KARTA>)formatterLocal.Deserialize(ms);
                            primljeneKarte.AddRange(noveKarte);

                            Console.WriteLine($"\nPrimili ste {noveKarte.Count} novih karata od servera!");
                            PrikaziKarte();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Nepoznat tip poruke od servera: {type}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Greška u komunikaciji sa serverom: " + ex.Message);
                    igraTraje = false;
                    potezAktivan = false;
                    break;
                }
            }
        }
    }
}
