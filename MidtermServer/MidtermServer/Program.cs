using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MidtermServer
{
    class Program
    {
        static Socket TCPserver;
        static Socket UDPserver;
        static byte[] TCPbuffer = new byte[512];
        static byte[] UDPbuffer = new byte[512];
        static float[] pos;

        static List<Socket> connectedClients = new List<Socket>();
        static List<IPEndPoint> UDPEndPoints = new List<IPEndPoint>();

        static void StartServer() //Initializes the server to the localhost address and begins waiting for connections.
        {
            //Setup our server  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEP = new IPEndPoint(ip, 8888);
            IPEndPoint serverEPUdp = new IPEndPoint(ip, 8889);

            TCPserver = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            TCPserver.Bind(serverEP);
            //Start listening for connection requests.
            TCPserver.Listen(8);

            UDPserver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            UDPserver.Bind(serverEPUdp);

            // Accept connections 
            Console.WriteLine("Waiting for connections...");

            TCPserver.BeginAccept(new AsyncCallback(AcceptTCPCallback), null);
            UDPserver.BeginReceive(UDPbuffer, 0, UDPbuffer.Length, 0, ReceiveUDPCallback, UDPserver);
        }

        static void AcceptTCPCallback(IAsyncResult result)
        {
            Socket client = TCPserver.EndAccept(result);
            connectedClients.Add(client);
            Console.WriteLine("Client connected! IP: {0}", client.RemoteEndPoint.ToString());
            
            client.BeginReceive(TCPbuffer, 0, TCPbuffer.Length, 0, new AsyncCallback(ReceiveTCPCallback), client);
            TCPserver.BeginAccept(new AsyncCallback(AcceptTCPCallback), null);
        }

        static void ReceiveTCPCallback(IAsyncResult result)
        {
            Socket client = (Socket)result.AsyncState;
            int rec = client.EndReceive(result);

            byte[] data = new byte[rec];
            Array.Copy(TCPbuffer, data, rec);
            string msg = Encoding.ASCII.GetString(data);

            if(msg.Length >= 5 && msg.Substring(0, 5) == "UDP: ") //Handshake
            {
                //Get UDP EndPoint
                string UDPEndPoint = msg.Substring(5);
                string[] parts = UDPEndPoint.Split(',');
                IPAddress ip = IPAddress.Parse(parts[0]);
                IPEndPoint newEP = new IPEndPoint(ip, int.Parse(parts[1]));
                if (!UDPEndPoints.Contains(newEP))
                {
                    UDPEndPoints.Add(newEP);
                }

                //Send their player number
                int playerNum = connectedClients.Count; //We need to ensure the client sends their first TCP packet before the next client connects.
                string playerNumberString = "PN: " + playerNum;
                byte[] send = Encoding.UTF8.GetBytes(playerNumberString);
                client.BeginSend(send, 0, send.Length, 0, new AsyncCallback(SendTCPCallback), client);

                Console.WriteLine("Received handshake from {0}, granted player {1}", client.RemoteEndPoint.ToString(), playerNum.ToString());
            }
            else if (msg.Length >= 5 && msg.Substring(0, 5) == "msg: ")
            {
                string txtChat = msg.Substring(5);
                Console.WriteLine("Received message \"{0}\" from {1}", txtChat, client.RemoteEndPoint.ToString());

                byte[] send = Encoding.UTF8.GetBytes(msg);

                foreach (Socket connectedClient in connectedClients)
                {
                    if (connectedClient == client)
                    {
                        continue;
                    }
                    connectedClient.BeginSend(send, 0, send.Length, 0, new AsyncCallback(SendTCPCallback), connectedClient);
                    Console.WriteLine("Sent message \"{0}\" to {1}", txtChat, connectedClient.RemoteEndPoint.ToString());
                }
            }
            else if (msg == "quit")
            {
                Console.WriteLine("Disconnecting client: {0}", client.RemoteEndPoint.ToString());
                connectedClients.Remove(client);
                client.Close();
                byte[] send = Encoding.UTF8.GetBytes("quit");
                connectedClients[0].BeginSend(send, 0, send.Length, 0, new AsyncCallback(SendTCPCallback), connectedClients[0]);
                return;
            }
            else
            {
                Console.WriteLine("Client {0} sent unknown data", client.RemoteEndPoint.ToString());
            }

            client.BeginReceive(TCPbuffer, 0, TCPbuffer.Length, 0, new AsyncCallback(ReceiveTCPCallback), client);
        }

        static void ReceiveUDPCallback(IAsyncResult result)
        {
            int rec = UDPserver.EndReceive(result);

            byte[] data = new byte[rec];
            Array.Copy(UDPbuffer, data, rec);
            
            pos = new float[rec / 4];
            Buffer.BlockCopy(data, 0, pos, 0, rec);

            Console.WriteLine("Received Player: " + pos[0] + "X:" + pos[1] + " Y:" + pos[2] + " Z:" + pos[3]);

            byte[] send = new byte[pos.Length * sizeof(float)];
            Buffer.BlockCopy(pos, 0, send, 0, send.Length);

            foreach (EndPoint ipep in UDPEndPoints)
            {
                UDPserver.BeginSendTo(send, 0, send.Length, 0, ipep, SendUDPCallback, ipep);

                //UDPserver.SendTo(send, ipep.RemoteEndPoint);
            }
                UDPserver.BeginReceive(UDPbuffer, 0, UDPbuffer.Length, 0, ReceiveUDPCallback, UDPserver);
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

        public static int Main(String[] args)
        {
            StartServer();
            Console.ReadKey();
            return 0;
        }
    }
}