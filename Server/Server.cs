using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RealTimeProject
{
    internal class Server
    {
        static int gameState = 10;
        static int speed = 5;
        static int bufferSize = 1024;

        static void ExecuteCommands(string[] commands)
        {
            foreach (string command in commands)
            {
                switch (command)
                {
                    case "MoveRight":
                        gameState += speed;
                        break;
                    case "MoveLeft":
                        gameState -= speed;
                        break;
                }
            }
        }
        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[1];

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
                    byte[] buffer = new byte[bufferSize];
                    clientSock.Receive(buffer);
                    string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0');
                    Console.WriteLine(data);
                    string[] commands = data.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    ExecuteCommands(commands);
                    clientSock.Send(Encoding.Latin1.GetBytes(gameState.ToString()));
                    Console.WriteLine("Sent " + gameState);
                }
            }
        }
    }
}
