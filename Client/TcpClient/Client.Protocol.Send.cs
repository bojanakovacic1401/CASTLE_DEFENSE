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
        private static void SendAll(Socket s, byte[] data)
        {
            int sent = 0;
            while (sent < data.Length)
            {
                int n = s.Send(data, sent, data.Length - sent, SocketFlags.None);
                if (n <= 0) throw new SocketException((int)SocketError.ConnectionReset);
                sent += n;
            }
        }

        private static void SendFrame(Socket s, byte type, byte[] payload)
        {
            byte[] header = new byte[5];
            header[0] = type;
            BitConverter.GetBytes(payload.Length).CopyTo(header, 1);

            SendAll(s, header);
            SendAll(s, payload);
        }

        private static void SendText(Socket s, string text)
        {
            SendFrame(s, MSG_TEXT, Encoding.UTF8.GetBytes(text));
        }
    }
}
