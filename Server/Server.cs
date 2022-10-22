using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using GameState = System.Collections.Generic.Dictionary<string, int>;
using HistoryData = System.Tuple<System.DateTime, string, System.Collections.Generic.Dictionary<string, int>>;

namespace RealTimeProject
{
    internal class Server
    {
        const int bufferSize = 1024;
        const bool twoPlayers = false;
        const int simLag = 0;

        static int speed = 50;
        static bool compensateLag = false;
        static List<HistoryData> gameHistory = 
            new List<HistoryData>
            { new HistoryData
                (DateTime.Now, "",
                new GameState{["p1x"] = 50, ["p2x"] = 700, ["p1score"] = 0, ["p2score"] = 0})};
        static Socket clientSock1;
        static Socket clientSock2 = null;
        static TimeSpan[] pRTTs = new TimeSpan[] {TimeSpan.Zero, TimeSpan.Zero};
        //commands start with the player number. ex: "1Shoot"
        static GameState ExecuteCommand(string command, GameState prevGameState)
        {
            var nextGameState = new GameState(prevGameState);
            char player = command[0];
            command = command[1..];
            switch (command)
            {
                case "MoveRight":
                    nextGameState["p" + player + "x"] += speed;
                    break;
                case "MoveLeft":
                    nextGameState["p" + player + "x"] -= speed;
                    break;
                case "Shoot":
                    if (nextGameState["p1x"] + 25 > nextGameState["p2x"] &&
                        nextGameState["p1x"] + 25 < nextGameState["p2x"] + 50)
                    {
                        nextGameState["p" + player + "score"] += 1;
                    }
                    break;
                case "Grid":
                    if (speed == 50)
                        speed = 5;
                    else
                        speed = 50;
                    break;
                case "LagComp":
                    compensateLag = !compensateLag;
                    break;
            }
            return nextGameState;
        }
        //static void ExecuteCommands(string[] commands, int player, DateTime time)
        //{
            
        //    foreach (string command in commands)
        //    {
                
        //        }
        //        //gameHistory.Add(new HistoryData(), command, newGameState));

        //        string printMsg = "";
        //        foreach (HistoryData tgs in gameHistory.Skip(gameHistory.Count - 10))
        //        {
        //            printMsg += string.Format("{0}|{1}\n", tgs.Item1.ToString("mm.ss.fff"), JsonSerializer.Serialize(tgs.Item3));
        //        }
        //        //printMsg += gameStateHistory.Count;
        //        Console.WriteLine(printMsg);

        //    }
        //}

        async static void ManagePlayer(Socket sock, int player, DateTime time)
        {
            //get data
            byte[] buffer = new byte[bufferSize];
            sock.Receive(buffer);

            if (player == 1)
                await Task.Delay(simLag);

            string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0');
            //Console.WriteLine("player " + player + ": " +  data);
            string[] commands = data.Split(',', StringSplitOptions.RemoveEmptyEntries);

            int insertIndex = gameHistory.Count;
            while (time <= gameHistory[insertIndex - 1].Item1)
            {
                insertIndex--;
            }
            GameState prevGameState;
            if (insertIndex == gameHistory.Count)
            {
                prevGameState = gameHistory.Last().Item3;
                foreach (string command in commands)
                {
                    prevGameState = ExecuteCommand(player + command, prevGameState);
                    gameHistory.Add(new HistoryData(time, player + "" + command, prevGameState));
                }
            }
            else
            {
                prevGameState = new GameState(gameHistory[insertIndex - 1].Item3);
                for (int i = insertIndex; i < commands.Length; i++)
                {
                    prevGameState = ExecuteCommand(player + commands[i], prevGameState);
                    gameHistory.Insert(i, new HistoryData(time, player + commands[0], prevGameState));
                }
                for (int i = insertIndex + commands.Length; i < gameHistory.Count; i++)
                {
                    string commandToRedo = gameHistory[i].Item2;
                    GameState updatedGameState = ExecuteCommand(commandToRedo, gameHistory[i - 1].Item3);
                    gameHistory.Insert(i, new HistoryData(time, commandToRedo, updatedGameState));
                }
            }

            string printMsg = "";
            foreach (HistoryData tgs in gameHistory.Skip(gameHistory.Count - 10))
            {
                printMsg += string.Format("{0}|{1}\n", tgs.Item1.ToString("mm.ss.fff"), JsonSerializer.Serialize(tgs.Item3));
            }
            //printMsg += gameStateHistory.Count;
            Console.WriteLine(printMsg);

            //send update to both
            string gameStateString = JsonSerializer.Serialize(gameHistory.Last().Item3);
            byte[] sendData = Encoding.Latin1.GetBytes(gameStateString);

            DateTime sendTime;
            if (clientSock2 != null)
            { 
                clientSock2.Send(sendData);
                sendTime = DateTime.Now;
                clientSock2.Receive(new byte[1]);
                pRTTs[1] = DateTime.Now - sendTime;
            }

            await Task.Delay(simLag);

            clientSock1.Send(sendData);
            sendTime = DateTime.Now;
            clientSock1.Receive(new byte[1]);
            pRTTs[0] = DateTime.Now - sendTime;

            Console.WriteLine("p1 rtt: " + pRTTs[0].TotalMilliseconds + ", p2 rtt: " + pRTTs[1].TotalMilliseconds + ", comp: " + compensateLag);
        }

        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[1];
            //address = IPAddress.Parse("172.16.2.167");
            address = IPAddress.Parse("10.100.102.20");
            Console.WriteLine(address);

            Socket serverSock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSock.Bind(new IPEndPoint(address, 12345));
            Console.WriteLine("Binded Successfully");
            serverSock.Listen(10);

            Console.WriteLine("Waiting for first player");
            clientSock1 = serverSock.Accept();
            Console.WriteLine("First player " + clientSock1.RemoteEndPoint + " entered");

            clientSock1.Send(Encoding.Latin1.GetBytes("1"));
            DateTime sendTime = DateTime.Now;
            clientSock1.Receive(new byte[1]);
            pRTTs[0] = DateTime.Now - sendTime;

            if (twoPlayers)
            {
                Console.WriteLine("Waiting for second player");
                clientSock2 = serverSock.Accept();
                Console.WriteLine("Second player " + clientSock1.RemoteEndPoint + " entered");

                clientSock2.Send(Encoding.Latin1.GetBytes("2"));
                sendTime = DateTime.Now;
                clientSock2.Receive(new byte[1]);
                pRTTs[1] = DateTime.Now - sendTime;
            }

            Console.WriteLine(pRTTs[0].TotalMilliseconds + ", " + pRTTs[1].TotalMilliseconds);

            while (true)
            {
                //Thread.Sleep(200);
                // Get player commands and execute them
                if (clientSock1.Poll(1, SelectMode.SelectRead))
                {
                    ManagePlayer(clientSock1, 1, DateTime.Now - (pRTTs[0] / 2) * Convert.ToByte(compensateLag));
                }
                if (clientSock2 != null)
                {
                    if (clientSock2.Poll(1, SelectMode.SelectRead))
                    {
                        ManagePlayer(clientSock2, 2, DateTime.Now - (pRTTs[1] / 2) * Convert.ToByte(compensateLag));
                    }
                }
            }
        }
    }
}
