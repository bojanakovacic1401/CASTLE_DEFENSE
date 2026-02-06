using System;
using System.Collections.Generic;
using System.Linq;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static void SetupTracks()
        {
            int brojTraka = 2 * brojIgraca;
            trake = new List<TRAKA>();
            TRAKA.BOJA[] boje = { TRAKA.BOJA.plava, TRAKA.BOJA.zelena, TRAKA.BOJA.crvena };
            int indexBoje = 0;

            for (int i = 0; i < brojTraka; i++)
            {
                if (i > 0 && i % 2 == 0)
                    indexBoje = (indexBoje + 1) % boje.Length;

                TRAKA traka = new TRAKA(i + 1, boje[indexBoje]);
                trake.Add(traka);
            }

            Console.WriteLine("\n------KREIRANE TRAKE-----");
            foreach (TRAKA traka in trake)
            {
                Console.WriteLine($"Traka {traka.Broj} - Boja: {traka.Boja}");
            }
        }
    }
}
