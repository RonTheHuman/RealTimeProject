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

        static void ReadSocket(Socket sock)
        {
            byte[] buffer = new byte[bufferSize];
            sock.Receive(buffer);
            string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0');
            Console.WriteLine(data);
            string[] commands = data.Split(',', StringSplitOptions.RemoveEmptyEntries);
            ExecuteCommands(commands);
        }

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[1];

            Socket serverSock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSock.Bind(new IPEndPoint(address, 12345));
            Console.WriteLine("Binded Successfully");
            serverSock.Listen(10);

            Socket clientSock1;
            Socket clientSock2 = null;
            Console.WriteLine("Waiting for first player");
            clientSock1 = serverSock.Accept();
            Console.WriteLine("First player " + clientSock1.RemoteEndPoint + " entered. Wait for second? [y/n]");
            if (Console.Read() == 'y')
            {
                Console.WriteLine("Waiting for second player");
                clientSock2 = serverSock.Accept();
                Console.WriteLine("Second player " + clientSock1.RemoteEndPoint);
            }

            while (true)
            {
                //Thread.Sleep(200);
                bool updated = false;
                // Get player commands and execute them
                if (clientSock1.Poll(1, SelectMode.SelectRead))
                {
                    ReadSocket(clientSock1);
                    updated = true;
                }
                if (clientSock2 != null)
                {
                    if (clientSock2.Poll(1, SelectMode.SelectRead))
                    {
                        ReadSocket(clientSock2);
                        updated = true;
                    }
                }
                // Send update to both
                if (updated)
                {
                    clientSock1.Send(Encoding.Latin1.GetBytes(gameState.ToString()));
                    if (clientSock2 != null)
                        clientSock2.Send(Encoding.Latin1.GetBytes(gameState.ToString()));
                    Console.WriteLine("Sent " + gameState);
                }
            }
        }
    }
}
