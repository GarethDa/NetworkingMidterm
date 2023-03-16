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
        Socket server;
        byte[] buffer = new byte[512];
        float[] pos;

        List<Socket> connectedClients = new List<Socket>();

        void StartServer() //Initializes the server to the localhost address and begins waiting for connections.
        {
            //Setup our server  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEP = new IPEndPoint(ip, 8888);

            server = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(serverEP);
            //Start listening for connection requests.
            server.Listen(8);

            // Accept connections 
            Console.WriteLine("Waiting for connections...");

            server.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        void AcceptCallback(IAsyncResult result)
        {
            Socket client = server.EndAccept(result);
            connectedClients.Add(client);
            Console.WriteLine("Client connected! IP: {0}", client.RemoteEndPoint.ToString());

            client.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), client);
        }

        void ReceiveCallback(IAsyncResult result)
        {
            Socket client = (Socket)result.AsyncState;
            int rec = client.EndReceive(result);

            byte[] data = new byte[rec];
            Array.Copy(buffer, data, rec);
            string msg = Encoding.ASCII.GetString(data);

            if(msg == "quit")
            {
                connectedClients.Remove(client);
                client.Close();
                return;
            }

            pos = new float[rec / 4];
            Buffer.BlockCopy(buffer, 0, pos, 0, rec);

            //client.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), client);
            Console.WriteLine("Received X:" + pos[0] + " Y:" + pos[1] + " Z:" + pos[2]);

            byte[] send = new byte[pos.Length * sizeof(float)];
            Buffer.BlockCopy(pos, 0, send, 0, send.Length);

            foreach (Socket connectedClient in connectedClients)
            {
                if (connectedClient == client)
                {
                    continue;
                }
                connectedClient.BeginSend(send, 0, send.Length, 0, new AsyncCallback(SendCallback), connectedClient);
            }

            client.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), client);
        }

        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }

        public int Main(String[] args)
        {
            StartServer();
            Console.ReadKey();
            return 0;
        }
    }
}