using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MidtermServer
{
    class Program
    {
        public static void StartServer()
        {
            //Setup our server  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEP = new IPEndPoint(ip, 8888);

            Socket server = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(serverEP);
            server.Listen(1);

            // Accept connections 
            Console.WriteLine("Waiting for connections...");

            Socket client = server.Accept();
            Console.WriteLine("Client connected!");
            IPEndPoint clientEP = (IPEndPoint)client.RemoteEndPoint;
            Console.WriteLine("Client: {0}  Port: {1}", clientEP.Address, clientEP.Port);

            byte[] msg = Encoding.ASCII.GetBytes("This is my first TCP server!!!!! Welcome to INFR3830");

            // Sending data to connected client 
            client.Send(msg);
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static int Main(String[] args)
        {
            StartServer();
            Console.ReadKey();
            return 0;
        }
    }
}