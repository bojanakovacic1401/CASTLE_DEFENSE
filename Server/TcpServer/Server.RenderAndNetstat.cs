using System;
using System.Diagnostics;
using System.Linq;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void PrikaziNetstatInformacije()
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("NETSTAT INFORMACIJE - TRENUTNE KONEKCIJE");
            Console.WriteLine(new string('=', 70));

            try
            {
                Process netstat = new Process();
                netstat.StartInfo.FileName = "netstat";
                netstat.StartInfo.Arguments = "-an | findstr :5555 :6666";
                netstat.StartInfo.RedirectStandardOutput = true;
                netstat.StartInfo.UseShellExecute = false;
                netstat.StartInfo.CreateNoWindow = true;

                netstat.Start();

                string output = netstat.StandardOutput.ReadToEnd();
                netstat.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                {
                    string[] lines = output.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.Contains(":5555") || line.Contains(":6666"))
                            Console.WriteLine(line.Trim());
                    }
                }
                else
                {
                    Console.WriteLine("Nema aktivnih konekcija na server portovima.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri pozivu netstat: {ex.Message}");
            }

            Console.WriteLine(new string('=', 70));
        }

        private static void IspisiStanjeTraka()
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("TRENUTNO STANJE TRAKA:");
            Console.WriteLine(new string('=', 60));

            int ukupnoProtivnika = 0;
            int ukupnoZidina = 0;

            foreach (TRAKA traka in trake)
            {
                ukupnoZidina += traka.brojZidina;
                ukupnoProtivnika += traka.suma.Count + traka.strelacZona.Count + traka.vitezZona.Count + traka.macevalacZona.Count;

                Console.WriteLine($"\nTraka {traka.Broj} ({traka.Boja}):");
                Console.WriteLine($"Zidovi: {traka.brojZidina}");

                if (traka.suma.Count > 0)
                {
                    Console.WriteLine($"Šuma ({traka.suma.Count}):");
                    foreach (PROTIVNIK p in traka.suma)
                        Console.WriteLine($"- {p.Ime} (HP: {p.ZivotniPoeni})");
                }

                if (traka.strelacZona.Count > 0)
                {
                    Console.WriteLine($"Strelac zona ({traka.strelacZona.Count}):");
                    foreach (PROTIVNIK p in traka.strelacZona)
                        Console.WriteLine($"- {p.Ime} (HP: {p.ZivotniPoeni})");
                }

                if (traka.vitezZona.Count > 0)
                {
                    Console.WriteLine($"Vitez zona ({traka.vitezZona.Count}):");
                    foreach (PROTIVNIK p in traka.vitezZona)
                        Console.WriteLine($"- {p.Ime} (HP: {p.ZivotniPoeni})");
                }

                if (traka.macevalacZona.Count > 0)
                {
                    Console.WriteLine($"  Mačevalac zona ({traka.macevalacZona.Count}):");
                    foreach (PROTIVNIK p in traka.macevalacZona)
                        Console.WriteLine($"- {p.Ime} (HP: {p.ZivotniPoeni})");
                }

                if (traka.suma.Count == 0 && traka.strelacZona.Count == 0 &&
                    traka.vitezZona.Count == 0 && traka.macevalacZona.Count == 0)
                {
                    Console.WriteLine(" (prazno)");
                }
            }

            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"UKUPNO: {ukupnoProtivnika} protivnika, {ukupnoZidina} zidina");
            Console.WriteLine(new string('=', 60));
        }
    }
}
