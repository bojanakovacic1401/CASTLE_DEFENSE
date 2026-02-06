using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteka
{
    [Serializable]
    public class ProtivnikKolicina
    {
        public PROTIVNIK Protivnik { get; set; }
        public int Kolicina { get; set; }

        public ProtivnikKolicina(PROTIVNIK p, int kolicina)
        {
            Protivnik = p;
            Kolicina = kolicina;
        }
        public ProtivnikKolicina() { }
    }
}

