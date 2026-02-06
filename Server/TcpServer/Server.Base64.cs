using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
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

        private static T DeserializeFromBase64<T>(string b64)
        {
            byte[] data = Convert.FromBase64String(b64);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                return (T)bf.Deserialize(ms);
            }
        }
#pragma warning restore SYSLIB0011
    }
}
