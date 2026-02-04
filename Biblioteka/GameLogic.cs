using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{
    public class GameLogic
    {
        public static List<PROTIVNIK> SviProtivnici = new()
        {
            new PROTIVNIK("Goblin", 1),
            new PROTIVNIK("Ork", 2),
            new PROTIVNIK("Trol", 3),
            new PROTIVNIK("Veliki kamen", 0),
            new PROTIVNIK("Prelaze u sledeću traku", 0),
            new PROTIVNIK("Kuga! Strelci", 0),
            new PROTIVNIK("Postavi još četiri protivnika", 0),
            new PROTIVNIK("Pomeranje plavih", 0),
            new PROTIVNIK("Pomeranje zelenih", 0),
            new PROTIVNIK("Pomeranje crvenih", 0),
        };

        public static List<KARTA> GenerisiSveKarteSaKopijama() {

            List<KARTA> sveKarte = new List<KARTA>();

            for (int i = 0; i < 3; i++)
            {
                sveKarte.Add(new KARTA("Strelac (plava)", "Udara jednog protivnika u strelac zoni"));
                sveKarte.Add(new KARTA("Strelac (crvena)", "Udara jednog protivnika u strelac zoni"));
                sveKarte.Add(new KARTA("Strelac (zelena)", "Udara jednog protivnika u strelac zoni"));
            }
            sveKarte.Add(new KARTA("Strelac (ljubicasta)", "Udara jednog protivnika u strelac zoni"));

            for (int i = 0; i < 3; i++)
            {
                sveKarte.Add(new KARTA("Vitez (plava)", "Udara jednog protivnika u vitez zoni"));
                sveKarte.Add(new KARTA("Vitez (crvena)", "Udara jednog protivnika u vitez zoni"));
                sveKarte.Add(new KARTA("Vitez (zelena)", "Udara jednog protivnika u vitez zoni"));
            }
            sveKarte.Add(new KARTA("Vitez (ljubicasta)", "Udara jednog protivnika u vitez zoni"));

            for (int i = 0; i < 3; i++)
            {
                sveKarte.Add(new KARTA("Macevalac (plava)", "Udara jednog protivnika u macevalac zoni"));
                sveKarte.Add(new KARTA("Macevalac (crvena)", "Udara jednog protivnika u macevalac zoni"));
                sveKarte.Add(new KARTA("Macevalac (zelena)", "Udara jednog protivnika u macevalac zoni"));
            }
            sveKarte.Add(new KARTA("Macevalac (ljubicasta)", "Udara jednog protivnika u macevalac zoni"));

            sveKarte.Add(new KARTA("Heroj (plava)", "Udara bilo kog protivnika"));
            sveKarte.Add(new KARTA("Heroj (crvena)", "Udara bilo kog protivnika"));
            sveKarte.Add(new KARTA("Heroj (zelena)", "Udara bilo kog protivnika"));

            sveKarte.Add(new KARTA("Varvarin (ljubicasta)", "Eliminiše jednog protivnika u bilo kojoj zoni"));

            for (int i = 0; i < 2; i++)
                sveKarte.Add(new KARTA("Katran (ljubicasta)", "Protivnik se ne pomera"));

            for (int i = 0; i < 4; i++)
            {
                sveKarte.Add(new KARTA("Cigla (ljubicasta)", "Koristi se zajedno sa Malterom"));
                sveKarte.Add(new KARTA("Malter (ljubicasta)", "Koristi se zajedno sa Ciglom"));
            }

            return sveKarte;

        } 

        public static List<ProtivnikKolicina> BazniProtivnici = new()
        {
            new ProtivnikKolicina(new PROTIVNIK("Goblin", 1), 12),
            new ProtivnikKolicina(new PROTIVNIK("Ork", 2), 11),
            new ProtivnikKolicina(new PROTIVNIK("Trol", 3), 8),
            new ProtivnikKolicina(new PROTIVNIK("Veliki kamen", 0), 4),
            new ProtivnikKolicina(new PROTIVNIK("Prelaze u sledeću traku", 0), 1),
            new ProtivnikKolicina(new PROTIVNIK("Kuga! Strelci", 0), 2),
            new ProtivnikKolicina(new PROTIVNIK("Postavi još četiri protivnika", 0), 1),
            new ProtivnikKolicina(new PROTIVNIK("Pomeranje plavih", 0), 2),
            new ProtivnikKolicina(new PROTIVNIK("Pomeranje zelenih", 0), 2),
            new ProtivnikKolicina(new PROTIVNIK("Pomeranje crvenih", 0), 2),
        };

        public static List<PROTIVNIK> IzaberiProtivnike(int brojIgraca)
        {
            List<PROTIVNIK> rezultat = new();

            foreach (ProtivnikKolicina pk in BazniProtivnici)
            {
                int konacnaKolicina = pk.Kolicina;

                if (brojIgraca == 1 && (pk.Protivnik.Ime == "Pomeranje zelenih" || pk.Protivnik.Ime == "Pomeranje crvenih"))
                    continue;

                if (brojIgraca == 2 && pk.Protivnik.Ime == "Pomeranje crvenih")
                    continue;

                if (brojIgraca == 1)
                {
                    if (pk.Kolicina <= 4)
                        konacnaKolicina = 1;
                    else
                        konacnaKolicina = pk.Kolicina / 3;
                }
                else if (brojIgraca == 2)
                {
                    if (pk.Kolicina == 1)
                        konacnaKolicina = 1;
                    else
                        konacnaKolicina = pk.Kolicina / 2;
                }

                for (int i = 0; i < konacnaKolicina; i++)
                {
                    rezultat.Add(new PROTIVNIK(pk.Protivnik.Ime, pk.Protivnik.ZivotniPoeni));
                }
            }
            return rezultat;
        }

        public static List<KARTA> IzaberiKarte(int brojIgraca)
        {
            List<KARTA> sveKarteSaKopijama = GenerisiSveKarteSaKopijama();
            List<KARTA> filtriraneKarte = new List<KARTA>();

            foreach (KARTA karta in sveKarteSaKopijama)
            {
                bool dodajKartu = false;

                if (brojIgraca == 1)
                {
                    if (karta.Naziv.Contains("(plava)") || karta.Naziv.Contains("(ljubicasta)"))
                        dodajKartu = true;
                }
                else if (brojIgraca == 2)
                {
                    if (karta.Naziv.Contains("(plava)") ||
                        karta.Naziv.Contains("(zelena)") ||
                        karta.Naziv.Contains("(ljubicasta)"))
                        dodajKartu = true;
                }
                else if (brojIgraca == 3)
                {
                    dodajKartu = true;
                }

                if (dodajKartu)
                    filtriraneKarte.Add(karta);
            }

            int limit = (brojIgraca == 1) ? 2 : (brojIgraca == 2) ? 3 : int.MaxValue;

            if (limit != int.MaxValue)
            {
                List<KARTA> cigle = filtriraneKarte.Where(k => k.Naziv.Contains("Cigla")).Take(limit).ToList();
                List<KARTA> malteri = filtriraneKarte.Where(k => k.Naziv.Contains("Malter")).Take(limit).ToList();

                filtriraneKarte.RemoveAll(k => k.Naziv.Contains("Cigla") || k.Naziv.Contains("Malter"));
                filtriraneKarte.AddRange(cigle);
                filtriraneKarte.AddRange(malteri);
            }


            return filtriraneKarte;
        }

    }
}
