using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using GameState = System.Collections.Generic.Dictionary<string, int>;
using TimedGameState = System.Tuple<System.DateTime, System.Collections.Generic.Dictionary<string, int>>;

namespace RealTimeProject
{
    internal class Server
    {
        const int speed = 50;
        const int bufferSize = 1024;
        const bool twoPlayers = false;
        const int simLag = 0;

        static List<TimedGameState> gameStateHistory = 
            new List<TimedGameState>
            { new TimedGameState
                (DateTime.Now, 
                new GameState{["p1x"] = 50, ["p2x"] = 700, ["p1score"] = 0, ["p2score"] = 0})};
        static Socket clientSock1;
        static Socket clientSock2 = null;

        static void ExecuteCommands(string[] commands, int player)
        {
            foreach (string command in commands)
            {
                var newGameState = new GameState(gameStateHistory.Last().Item2);
                switch (command)
                {
                    case "MoveRight":
                        newGameState["p" + player + "x"] += speed;
                        break;
                    case "MoveLeft":
                        newGameState["p" + player + "x"] -= speed;
                        break;
                    case "Shoot":
                        if (newGameState["p1x"] + 25 > newGameState["p2x"] &&
                            newGameState["p1x"] + 25 < newGameState["p2x"] + 50)
                        {
                            newGameState["p" + player + "score"] += 1;
                        }
                        break;
                }
                gameStateHistory.Add(new Tuple<DateTime, Dictionary<string, int>>(DateTime.Now, newGameState));
                //Console.WriteLine(DateTime.Now.ToString("mm:ss.fff") +
                //    "| Recieved: " + command + ", P" + player + ", State: " + JsonSerializer.Serialize(gameState));
                Console.Clear();
                gameStateHistory.ForEach(tgs => Console.Write("{0}|{1}\n", 
                    tgs.Item1.ToString("mm.ss.fff"), JsonSerializer.Serialize(tgs.Item2)));
            }
        }

        async static void ManagePlayer(Socket sock, int player)
        {
            //get data
            byte[] buffer = new byte[bufferSize];
            sock.Receive(buffer);

            if (player == 1)
                await Task.Delay(simLag);

            string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0');
            //Console.WriteLine("player " + player + ": " +  data);
            string[] commands = data.Split(',', StringSplitOptions.RemoveEmptyEntries);

            //update gamestate
            ExecuteCommands(commands, player);

            //send update to both
            string gameStateString = JsonSerializer.Serialize(gameStateHistory.Last().Item2);
            byte[] sendData = Encoding.Latin1.GetBytes(gameStateString);
            if (clientSock2 != null)
                clientSock2.Send(sendData);

            await Task.Delay(simLag);

            clientSock1.Send(sendData);
        }

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[1];
            address = IPAddress.Parse("172.16.2.167");
            Console.WriteLine(address);

            Socket serverSock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSock.Bind(new IPEndPoint(address, 12345));
            Console.WriteLine("Binded Successfully");
            serverSock.Listen(10);

            Console.WriteLine("Waiting for first player");
            clientSock1 = serverSock.Accept();
            Console.WriteLine("First player " + clientSock1.RemoteEndPoint + " entered");
            clientSock1.Send(Encoding.Latin1.GetBytes("1"));
            if (twoPlayers)
            {
                Console.WriteLine("Waiting for second player");
                clientSock2 = serverSock.Accept();
                clientSock2.Send(Encoding.Latin1.GetBytes("2"));
                Console.WriteLine("Second player " + clientSock1.RemoteEndPoint);
            }

            while (true)
            {
                //Thread.Sleep(200);
                bool updated = false;
                // Get player commands and execute them
                if (clientSock1.Poll(1, SelectMode.SelectRead))
                {
                    ManagePlayer(clientSock1, 1);
                    updated = true;
                }
                if (clientSock2 != null)
                {
                    if (clientSock2.Poll(1, SelectMode.SelectRead))
                    {
                        ManagePlayer(clientSock2, 2);
                        updated = true;
                    }
                }
            }
        }
    }
}
