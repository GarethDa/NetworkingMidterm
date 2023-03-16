using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MidtermServer
{
    class Program
    {
        Socket TCPserver;
        Socket UDPserver;
        byte[] TCPbuffer = new byte[512];
        byte[] UDPbuffer = new byte[512];
        float[] pos;

        List<Socket> connectedClients = new List<Socket>();

        void StartServer() //Initializes the server to the localhost address and begins waiting for connections.
        {
            //Setup our server  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEP = new IPEndPoint(ip, 8888);

            TCPserver = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            TCPserver.Bind(serverEP);
            //Start listening for connection requests.
            TCPserver.Listen(8);

            UDPserver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UDPserver.Bind(serverEP);

            // Accept connections 
            Console.WriteLine("Waiting for connections...");

            TCPserver.BeginAccept(new AsyncCallback(AcceptTCPCallback), null);
            UDPserver.BeginReceive(UDPbuffer, 0, UDPbuffer.Length, 0, ReceiveUDPCallback, 0);
        }

        void AcceptTCPCallback(IAsyncResult result)
        {
            Socket client = TCPserver.EndAccept(result);
            connectedClients.Add(client);
            Console.WriteLine("Client connected! IP: {0}", client.RemoteEndPoint.ToString());

            client.BeginReceive(TCPbuffer, 0, TCPbuffer.Length, 0, new AsyncCallback(ReceiveTCPCallback), client);
        }

        void ReceiveTCPCallback(IAsyncResult result)
        {
            Socket client = (Socket)result.AsyncState;
            int rec = client.EndReceive(result);

            byte[] data = new byte[rec];
            Array.Copy(TCPbuffer, data, rec);
            string msg = Encoding.ASCII.GetString(data);

            if (msg.Substring(0, 5) == "msg: ")
            {
                string txtChat = msg.Substring(5);
                Console.WriteLine("Received: {0}", txtChat);

                byte[] send = Encoding.UTF8.GetBytes(txtChat);

                foreach (Socket connectedClient in connectedClients)
                {
                    if (connectedClient == client)
                    {
                        continue;
                    }
                    connectedClient.BeginSend(send, 0, send.Length, 0, new AsyncCallback(SendTCPCallback), connectedClient);
                }
            }
            else if (msg == "quit")
            {
                connectedClients.Remove(client);
                client.Close();
                return;
            }
            else
            {
                Console.WriteLine("Client {0} sent unknown data", client.RemoteEndPoint.ToString());
            }

            client.BeginReceive(TCPbuffer, 0, TCPbuffer.Length, 0, new AsyncCallback(ReceiveTCPCallback), client);
        }

        void ReceiveUDPCallback(IAsyncResult result)
        {
            EndPoint ip = new IPEndPoint(IPAddress.Any, 0);
            int rec = UDPserver.EndReceiveFrom(result, ref ip);

            byte[] data = new byte[rec];
            Array.Copy(UDPbuffer, data, rec);
            
            pos = new float[rec / 4];
            Buffer.BlockCopy(UDPbuffer, 0, pos, 0, rec);

            Console.WriteLine("Received X:" + pos[0] + " Y:" + pos[1] + " Z:" + pos[2]);

            byte[] send = new byte[pos.Length * sizeof(float)];
            Buffer.BlockCopy(pos, 0, send, 0, send.Length);

            foreach (Socket connectedClient in connectedClients)
            {
                if (connectedClient.RemoteEndPoint == ip)
                {
                    continue;
                }
                UDPserver.BeginSendTo(send, 0, send.Length, 0, connectedClient.RemoteEndPoint, SendUDPCallback, connectedClient.RemoteEndPoint);
            }

            UDPserver.BeginReceive(UDPbuffer, 0, UDPbuffer.Length, 0, ReceiveUDPCallback, 0);
        }

        private static void SendTCPCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }

        private static void SendUDPCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSendTo(result);
        }

        public int Main(String[] args)
        {
            StartServer();
            Console.ReadKey();
            return 0;
        }
    }
}