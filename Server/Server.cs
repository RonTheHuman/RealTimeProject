using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RealTimeProject
{
    internal class Server
    {
        static void Main(string[] args)
        {
            Socket serverSock = new Socket(SocketType.Stream, ProtocolType.Tcp);

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[0];
            //foreach (IPAddress a in ipHost.AddressList){   
            //    if (a.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        adress = a;
            //        break;
            //    }
            //}
            serverSock.Bind(new IPEndPoint(address, 0));
            Console.WriteLine("Binded Successfully");
            serverSock.Listen(10);
            while (true)
            {
                Console.WriteLine("Waiting for conneciton");
                Socket clientSock = serverSock.Accept();
                while (true)
                {
                string msg = Console.ReadLine();
                clientSock.Send(Encoding.Latin1.GetBytes(msg));
                }
            }
        }
    }
}
