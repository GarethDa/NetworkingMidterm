using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
namespace AsyncClient
{
    class Program
    {
        private static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] buffer = new byte[512];
        private static byte[] bpos;
        private static float[] pos;


        private static void ReceiveCallback(IAsyncResult results)
        {
            Socket socket = (Socket)results.AsyncState;
            int rec = socket.EndReceive(results);
            //lec06
            pos = new float[rec / 4];
            Buffer.BlockCopy(buffer, 0, pos, 0, rec);
            Console.WriteLine("Client recv X:" + pos[0] + " Y:" + pos[1] + " Z:" +
pos[2]);
            //lec05
            //byte[] data = new byte[rec];
            //Array.Copy(buffer, data, rec);
            //String msg = Encoding.ASCII.GetString(data);
            //Console.WriteLine("Recv: " + msg);
            socket.BeginReceive(buffer, 0, buffer.Length, 0,
                new AsyncCallback(ReceiveCallback), socket);
        }
        private static void SyncSend()
        {
            int c = 0;
            //lec06
            float x, y, z = 0;

            while (true)
            {
                //lec06
                x = y = ++z;
                pos = new float[] { x, y, z };
                bpos = new byte[pos.Length * 4];
                Buffer.BlockCopy(pos, 0, bpos, 0, bpos.Length);
                //lec05
                //byte[] buffer = Encoding.ASCII.GetBytes(c.ToString());

                //lec05
                //client.Send(buffer);

                client.Send(bpos);
                c++;
                Thread.Sleep(1000);
            }
        }
        static void Main(string[] args)
        {
            client.Connect(IPAddress.Parse("3.83.34.7"), 8888);
            Console.WriteLine("Connected to server!!!");
            client.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), client);
            SyncSend();
            Console.Read();
        }
    }
}