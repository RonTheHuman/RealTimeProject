using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RealTimeProject
{
    internal class Server
    {
        static int x = 10;
        static int speed = 5;
        static int bufferSize = 1024;
        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[0];
            foreach (IPAddress a in ipHost.AddressList)
            {
                Console.WriteLine(a);
            }
            Socket serverSock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSock.Bind(new IPEndPoint(address, 12345));
            Console.WriteLine("Binded Successfully");
            serverSock.Listen(10);
            while (true)
            {
                Console.WriteLine("Waiting for conneciton");
                Socket clientSock = serverSock.Accept();
                while (true)
                {
                    //Thread.Sleep(200);
                    byte[] data = new byte[bufferSize];
                    clientSock.Receive(data);
                    string action = Encoding.Latin1.GetString(data).TrimEnd('\0');
                    switch (action)
                    {
                        case "MoveRight":
                            x += speed;
                            break;
                        case "MoveLeft":
                            x -= speed;
                            break;
                    }
                    clientSock.Send(Encoding.Latin1.GetBytes(x.ToString()));
                    Console.WriteLine("Sent " + x);
                }
            }
        }
    }
}
