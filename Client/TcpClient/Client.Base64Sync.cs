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
#pragma warning disable SYSLIB0011
        private static string SerializeToBase64(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
#pragma warning restore SYSLIB0011

        private static void PosaljiZamenaSync(KARTA dataKarta, KARTA dobijenaKarta)
        {
            try
            {
                string b64Data = SerializeToBase64(dataKarta);
                string b64Dobijena = SerializeToBase64(dobijenaKarta);
                string poruka = $"ZAMENA_SYNC {b64Data} {b64Dobijena}";
                SendText(tcpKlijent, poruka);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri slanju ZAMENA_SYNC serveru: {ex.Message}");
            }
        }
    }
}
