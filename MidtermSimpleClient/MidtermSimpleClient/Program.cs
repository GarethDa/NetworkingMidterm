using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MidtermSimpleClient
{
    internal class Program
    {
        public class AsyncServer
        {
            private static byte[] buffer = new byte[512];
            private static Socket server;

            private static byte[] outBuffer = new byte[512];
            private static string outMsg = "";

            public static int Main(String[] args)
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPAddress ip = IPAddress.Parse("127.0.0.1");

                server.Bind(new IPEndPoint(ip, 8888));

                server.Listen(8);

                server.BeginAccept(new AsyncCallback(AcceptCallback), null);

                Console.Read();
            }

            private static void AcceptCallback(IAsyncResult result)
            {
                
            }
        }
    }
}
