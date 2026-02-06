using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using Biblioteka;

namespace CastleDefensePR.Server
{
    // This project uses partial class to split your original Server.cs into smaller files
    public partial class Server
    {
        private static List<TRAKA> trake;
        private static readonly HashSet<Socket> zamenaSyncDone = new HashSet<Socket>();

        private static object lockObject = new object();
        private static Dictionary<Socket, bool> potezZavrsen = new Dictionary<Socket, bool>();
        private static Dictionary<Socket, bool> kartuOdigrao = new Dictionary<Socket, bool>();
        private static Dictionary<Socket, int> trazeneKarte = new Dictionary<Socket, int>();
        private static Dictionary<Socket, List<KARTA>> karteIgraca = new Dictionary<Socket, List<KARTA>>();
        private static List<KARTA> dostupneKarte;
        private static List<PROTIVNIK> preostaliProtivnici;
        private static int brojIgraca;
        private static Socket zamenaOd = null;
        private static Socket zamenaKa = null;
        private static KARTA kartaZaZamenu = null;
        private static bool zamenaUToku = false;
        private static Dictionary<Socket, bool> kartaIzbacena = new Dictionary<Socket, bool>();
        private static List<Socket> tcpKlijenti = new List<Socket>();
        private static bool zamenaVecIskoriscenaUTomPotezu = false;
        private static Dictionary<PROTIVNIK, bool> protivniciZalepljeniKatranom = new Dictionary<PROTIVNIK, bool>();

        // === TCP FRAME PROTOKOL (server -> client) ===
        private const byte MSG_TEXT = 1;
        private const byte MSG_CARDS = 2;

        // === TCP FRAME RECEIVE (client -> server) ===
        private const int MAX_FRAME = 10_000_000; // 10 MB limit
        private static readonly Dictionary<Socket, List<byte>> recvBuffers = new();
    }
}
