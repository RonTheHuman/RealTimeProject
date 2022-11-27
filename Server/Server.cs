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
        const bool twoPlayers = false;
        const int bufferSize = 1024;
        const int simLag = 0;

        static int speed = 50;
        static bool compensateLag = true;
        static List<HistoryData> gameHistory =
            new List<HistoryData>
            { new HistoryData
                (DateTime.Now, "",
                new GameState{["p1x"] = 50, ["p2x"] = 50, ["p1score"] = 0, ["p2score"] = 0})
            };
        //static List<HistoryData> gameHistory = 
        //    new List<HistoryData> {
        //    new HistoryData
        //        (DateTime.Now, "",
        //        new GameState{["p1x"] = 50, ["p2x"] = 50, ["p1score"] = 0, ["p2score"] = 0}),
        //    new HistoryData
        //        (DateTime.Now + new TimeSpan(0, 0, 0, 2), "2Shoot",
        //        new GameState{["p1x"] = 50, ["p2x"] = 50, ["p1score"] = 0, ["p2score"] = 1}),
        //};
        static Socket client1Sock, client1SockEcho, client2Sock, client2SockEcho;
        static TimeSpan[] pRTTs = new TimeSpan[] {TimeSpan.Zero, TimeSpan.Zero};
        static Action<Socket, TimeSpan[], int> getRTT = (Socket sock, TimeSpan[] pRTTs, int i) =>
        {
            sock.Send(new byte[1]);
            Console.WriteLine("[sent echo]");
            DateTime sendTime = DateTime.Now;
            sock.Receive(new byte[1]);
            Console.WriteLine("[recvd echo]");
            pRTTs[i] = DateTime.Now - sendTime;
            Console.WriteLine("[p" + (i+1) + " rtt: " + pRTTs[0].TotalMilliseconds +  ", comp: " + compensateLag + "]");
        };
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
            }
            return nextGameState;
        }

        async static void ManagePlayer(Socket sock, int player, DateTime time)
        {
            //get data
            byte[] buffer = new byte[bufferSize];
            sock.Receive(buffer);

            if (player == 1)
                await Task.Delay(simLag);

           string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0');
            Console.WriteLine("\n\nfrom p" + player + ": " + data);
            string[] commands = data.Split(',', StringSplitOptions.RemoveEmptyEntries);

            //first handle commands that don't change gamestate
            foreach (string command in commands)
            {
                switch (command)
                {
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
            }

            //lag compensation
            int insertIndex = gameHistory.Count;
            while (time <= gameHistory[insertIndex - 1].Item1)
            {
                insertIndex--;
            }
            GameState prevGameState;
            if (insertIndex == gameHistory.Count)
            {
                Console.WriteLine("no redoing needed");
                prevGameState = gameHistory.Last().Item3;
                foreach (string command in commands)
                {
                    prevGameState = ExecuteCommand(player + command, prevGameState);
                    gameHistory.Add(new HistoryData(time, player + "" + command, prevGameState));
                }
            }
            else
            {
                Console.WriteLine("redoing needed");
                prevGameState = new GameState(gameHistory[insertIndex - 1].Item3);
                for (int i = insertIndex; i < insertIndex + commands.Length; i++)
                {
                    prevGameState = ExecuteCommand(player + commands[i - insertIndex], prevGameState);
                    gameHistory.Insert(i, new HistoryData(time, player + commands[0], prevGameState));
                }
                for (int i = insertIndex + commands.Length; i < gameHistory.Count; i++)
                {
                    string commandToRedo = gameHistory[i].Item2;
                    GameState updatedGameState = ExecuteCommand(commandToRedo, gameHistory[i - 1].Item3);
                    gameHistory[i] =  new HistoryData(time, commandToRedo, updatedGameState);
                }
            }

            //send update to both
            string gameStateString = JsonSerializer.Serialize(gameHistory.Last().Item3);
            byte[] sendData = Encoding.Latin1.GetBytes(gameStateString);

            DateTime sendTime;
            if (client2Sock != null)
            { 
                Task.Factory.StartNew(() => getRTT(client2SockEcho, pRTTs, 1));
                client2Sock.Send(sendData);
            }

            await Task.Delay(simLag);

            Task.Factory.StartNew(() => getRTT(client1SockEcho, pRTTs, 0));
            client1Sock.Send(sendData);

            string printMsg = "New history:\n";
            foreach (HistoryData hd in gameHistory.Skip(gameHistory.Count - 10))
            {
                printMsg += string.Format("{0}|{1}\n", hd.Item1.ToString("mm.ss.fff"), JsonSerializer.Serialize(hd.Item3));
            }
            Console.WriteLine(printMsg);
        }

        static void Main(string[] args)
        {
            //get address, either auto or home or school
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[1];
            address = IPAddress.Parse("172.16.2.167");
            //address = IPAddress.Parse("10.100.102.20");
            //Console.WriteLine(address);

            //create data and echo socket
            Socket serverSock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSock.Bind(new IPEndPoint(address, 12345));
            Socket serverSockEcho = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSockEcho.Bind(new IPEndPoint(address, 12346));
            Console.WriteLine("Binded Successfully");
            serverSock.Listen(10);
            serverSockEcho.Listen(10);

            Console.WriteLine("Waiting for first player");
            client1Sock = serverSock.Accept();
            client1SockEcho = serverSockEcho.Accept();
            Console.WriteLine("First player " + client1Sock.RemoteEndPoint + " entered");

            Task.Factory.StartNew(() => getRTT(client1SockEcho, pRTTs, 0));
            client1Sock.Send(Encoding.Latin1.GetBytes("1"));

            if (twoPlayers)
            {
                Console.WriteLine("Waiting for second player");
                client2Sock = serverSock.Accept();
                client2SockEcho = serverSockEcho.Accept();
                Console.WriteLine("Second player " + client1Sock.RemoteEndPoint + " entered");

                Task.Factory.StartNew(() => getRTT(client2SockEcho, pRTTs, 1));
                client2Sock.Send(Encoding.Latin1.GetBytes("2"));
            }
            //Console.WriteLine("p1 rtt: " + pRTTs[0].TotalMilliseconds + ", p2 rtt: " + pRTTs[1].TotalMilliseconds + ", comp: " + compensateLag);

            // If player sent message, manage it
            while (true)
            {
                //Thread.Sleep(200);
                if (client1Sock.Poll(1, SelectMode.SelectRead))
                {
                    var manageStart = DateTime.Now;
                    //Console.WriteLine("\nmanageStart: " + DateTime.Now.ToString("mm.ss.fff"));
                    ManagePlayer(client1Sock, 1, DateTime.Now - (pRTTs[0] / 2) * Convert.ToByte(compensateLag));
                    //Console.WriteLine("manageEnd: " + DateTime.Now.ToString("mm.ss.fff") + ", " + (DateTime.Now - manageStart).TotalMilliseconds + " ms to manage messages");
                }
                if (client2Sock != null)
                {
                    if (client2Sock.Poll(1, SelectMode.SelectRead))
                    {
                        ManagePlayer(client2Sock, 2, DateTime.Now - (pRTTs[1] / 2) * Convert.ToByte(compensateLag));
                    }
                }
            }
        }
    }
}
