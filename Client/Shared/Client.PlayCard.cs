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
        private static void OdigrajKartu()
        {
            if (kartuOdigraoUTrenutnomPotezu)
            {
                Console.WriteLine("Već ste odigrali kartu u ovom potezu! Možete samo da završite potez.");
                return;
            }

            if (primljeneKarte.Count == 0)
            {
                Console.WriteLine("Nemate nijednu kartu za odigravanje!");
                return;
            }

            bool imaKatran = primljeneKarte.Any(k => k.Naziv.Contains("Katran"));
            if (imaKatran)
            {
                Console.WriteLine("KATRAN KARTA: Ova karta sprečava protivnika da se pomera na kraju poteza.");
                Console.WriteLine("Efekat se automatski primenjuje prilikom odigravanja.");
            }

            bool imaCiglu = primljeneKarte.Any(k => k.Naziv.Contains("Cigla"));
            bool imaMalter = primljeneKarte.Any(k => k.Naziv.Contains("Malter"));

            if (imaCiglu && imaMalter)
            {
                Console.Write("Imate i Ciglu i Malter. Želite li da odigrate obe zajedno? (da/ne): ");
                string odgovor = Console.ReadLine().ToLower();
                if (odgovor == "da")
                {
                    Console.Write("Unesite broj trake na koju se primenjuje: ");
                    if (!int.TryParse(Console.ReadLine(), out int traka))
                    {
                        Console.WriteLine("Neispravan unos!");
                        return;
                    }

                    string poruka = $"ODIGRAO Cigla_Malter {traka}";
                    SendText(tcpKlijent, poruka);

                    var cigla = primljeneKarte.FirstOrDefault(k => k.Naziv.Contains("Cigla"));
                    var malter = primljeneKarte.FirstOrDefault(k => k.Naziv.Contains("Malter"));

                    List<string> uklonjeneKarte = new List<string>();

                    if (cigla != null)
                    {
                        primljeneKarte.Remove(cigla);
                        uklonjeneKarte.Add(cigla.Naziv);
                    }
                    if (malter != null)
                    {
                        primljeneKarte.Remove(malter);
                        uklonjeneKarte.Add(malter.Naziv);
                    }

                    Console.WriteLine($"Odigrali ste: {string.Join(" + ", uklonjeneKarte)}");

                    kartuOdigraoUTrenutnomPotezu = true;

                    PrikaziKarte();
                    return;
                }
            }

            Console.Write("Unesite broj karte koju želite da odigrate: ");
            if (!int.TryParse(Console.ReadLine(), out int broj))
            {
                Console.WriteLine("Neispravan unos!");
                return;
            }

            if (broj >= 1 && broj <= primljeneKarte.Count)
            {
                KARTA karta = primljeneKarte[broj - 1];
                string poruka = "";

                if (karta.Naziv.Contains("Cigla") || karta.Naziv.Contains("Malter"))
                {
                    Console.Write("Unesite broj trake na koju se primenjuje: ");
                    if (!int.TryParse(Console.ReadLine(), out int traka)) { Console.WriteLine("Neispravan unos!"); return; }
                    poruka = $"ODIGRAO {karta.Naziv} {traka}";
                }
                else if (karta.Naziv.Contains("Varvarin") || karta.Naziv.Contains("Heroj") || karta.Naziv.Contains("Katran"))
                {
                    Console.Write("Unesite broj trake: ");
                    if (!int.TryParse(Console.ReadLine(), out int traka)) { Console.WriteLine("Neispravan unos!"); return; }

                    Console.Write("Unesite zonu (1=strelac, 2=vitez, 3=macevalac, 4=suma): ");
                    if (!int.TryParse(Console.ReadLine(), out int zona)) { Console.WriteLine("Neispravan unos!"); return; }

                    Console.Write("Unesite redni broj protivnika (počev od 1): ");
                    if (!int.TryParse(Console.ReadLine(), out int protivnik)) { Console.WriteLine("Neispravan unos!"); return; }

                    poruka = $"ODIGRAO {karta.Naziv} {traka} {zona} {protivnik}";
                }
                else
                {
                    Console.Write("Unesite broj trake: ");
                    if (!int.TryParse(Console.ReadLine(), out int traka)) { Console.WriteLine("Neispravan unos!"); return; }

                    Console.Write("Unesite redni broj protivnika (počev od 1): ");
                    if (!int.TryParse(Console.ReadLine(), out int protivnik)) { Console.WriteLine("Neispravan unos!"); return; }

                    int zona = 1;
                    if (karta.Naziv.Contains("Vitez")) zona = 2;
                    else if (karta.Naziv.Contains("Mačevalac")) zona = 3;

                    poruka = $"ODIGRAO {karta.Naziv} {traka} {zona} {protivnik}";
                }

                Console.WriteLine($"Šaljem serveru: {poruka}");
                SendText(tcpKlijent, poruka);

                primljeneKarte.RemoveAt(broj - 1);
                kartuOdigraoUTrenutnomPotezu = true;

                Console.WriteLine($"Karta {karta.Naziv} je odigrana.");
                PrikaziKarte();
            }
            else
            {
                Console.WriteLine("Neispravan broj karte.");
            }
        }
    }
}
