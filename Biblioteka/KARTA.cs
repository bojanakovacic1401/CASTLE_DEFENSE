using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{
    [Serializable]
    public class KARTA
    {
        public string Naziv { get; set; }
        public string Efekat { get; set; }

        public KARTA() {}
        public KARTA(string naziv, string efekat)
        {
            Naziv = naziv;
            Efekat = efekat;
        }
    }
}
