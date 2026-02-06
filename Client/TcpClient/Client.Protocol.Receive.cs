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
        private static byte[] ReceiveExactly(Socket s, int size)
        {
            byte[] buf = new byte[size];
            int read = 0;
            while (read < size)
            {
                int n = s.Receive(buf, read, size - read, SocketFlags.None);
                if (n == 0) throw new SocketException((int)SocketError.ConnectionReset);
                read += n;
            }
            return buf;
        }

        private static (byte type, byte[] payload) ReceiveFrame(Socket s)
        {
            byte[] header = ReceiveExactly(s, 5);
            byte type = header[0];
            int len = BitConverter.ToInt32(header, 1);

            if (len < 0 || len > 10_000_000)
                throw new InvalidDataException("Neispravna dužina frame-a.");

            byte[] payload = ReceiveExactly(s, len);
            return (type, payload);
        }
    }
}
