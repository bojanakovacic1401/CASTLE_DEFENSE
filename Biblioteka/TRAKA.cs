using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{
    [Serializable]
    public class TRAKA
    {
        public TRAKA() { }
        public int Broj { get; set; }
        public enum BOJA { plava, crvena, zelena }
        public BOJA Boja { get; set; }
        public List<PROTIVNIK> suma { get; set; } = new List<PROTIVNIK>();
        public List<PROTIVNIK> strelacZona { get; set; } = new List<PROTIVNIK>();
        public List<PROTIVNIK> vitezZona { get; set; } = new List<PROTIVNIK>();
        public List<PROTIVNIK> macevalacZona { get; set; } = new List<PROTIVNIK>();
        public int brojZidina { get; set; } = 2;

        public TRAKA(int broj, BOJA boja)
        {
            Broj = broj;
            Boja = boja;
        }
    }
}
