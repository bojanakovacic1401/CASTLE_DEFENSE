using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Biblioteka;

namespace CastleDefensePR.Server
{
    public partial class Server
    {
        private static List<byte> GetRecvBuffer(Socket s)
        {
            if (!recvBuffers.TryGetValue(s, out var buf))
            {
                buf = new List<byte>(4096);
                recvBuffers[s] = buf;
            }
            return buf;
        }

        private static bool TryParseFrame(List<byte> buf, out byte type, out byte[] payload)
        {
            type = 0;
            payload = null;

            if (buf.Count < 5) return false;

            type = buf[0];
            int len = buf[1] | (buf[2] << 8) | (buf[3] << 16) | (buf[4] << 24);

            if (len < 0 || len > MAX_FRAME)
                throw new InvalidDataException("Neispravna dužina frame-a.");

            if (buf.Count < 5 + len) return false;

            payload = buf.GetRange(5, len).ToArray();
            buf.RemoveRange(0, 5 + len);
            return true;
        }

        private static void RemoveClientState(Socket s)
        {
            recvBuffers.Remove(s);
        }

        private static void SendAll(Socket s, byte[] data)
        {
            int sent = 0;
            while (sent < data.Length)
            {
                try
                {
                    int n = s.Send(data, sent, data.Length - sent, SocketFlags.None);
                    if (n <= 0) throw new SocketException((int)SocketError.ConnectionReset);
                    sent += n;
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock)
                {
                    var write = new List<Socket> { s };
                    Socket.Select(null, write, null, 1000 * 1000);
                }
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

        private static void SendCards(Socket s, List<KARTA> cards)
        {
#pragma warning disable SYSLIB0011
            BinaryFormatter bf = new BinaryFormatter();
#pragma warning restore SYSLIB0011
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, cards);
                SendFrame(s, MSG_CARDS, ms.ToArray());
            }
        }
    }
}
