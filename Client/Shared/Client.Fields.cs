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
        private static List<KARTA> primljeneKarte = new List<KARTA>();
        private static Socket udpRazmena;
        private static Socket tcpKlijent;
        private static bool potezAktivan = true;
        private static volatile bool udpAktivan = true;
        private static bool zamenaUradjena = false;
        private static int maxBrojKarata = 0;
        private static bool igraTraje = true;
        private static Thread serverThread;
        private static ManualResetEventSlim zamenaProveraEvent = new ManualResetEventSlim(false);
        private static volatile bool zamenaDozvoljena = false;

        // Flags for tracking turn state
        private static bool kartuOdigraoUTrenutnomPotezu = false;
        private static bool potezUToku = false;

        // Variables for handling swap requests
        private static object lockObject = new object();
        private static bool zahtevZaZamenu = false;
        private static KARTA primljenaKartaZaZamenu = null;
        private static EndPoint udpKlijentZaZamenu = null;

        // Formatter for card payloads
        private static BinaryFormatter formatter;

        private static bool kartaIzbacenaUTrenutnomPotezu = false;

        // TCP frame protocol
        private const byte MSG_TEXT = 1;
        private const byte MSG_CARDS = 2;
    }
}
