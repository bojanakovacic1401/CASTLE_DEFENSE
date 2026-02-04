using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{
    [Serializable]
    public class PROTIVNIK
    {
        public string Ime { get; set; }
        public int ZivotniPoeni { get; set; }

        public PROTIVNIK(string ime, int hp)
        {
            Ime = ime;
            ZivotniPoeni = hp;
        }
        public PROTIVNIK() {}
    }
}
