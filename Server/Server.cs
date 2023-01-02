using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace RealTimeProject
{
    internal class Server
    {
        const int bufferSize = 1024;
        const int pCount = 1;
        static bool grid = false;
        static bool compensateLag = true;
        //const int simLag = 0;

        static List<Frame> history =
            new List<Frame>
            { new Frame(new string[] {"0000", "0000"}, new GameState(new int[] {0, 0}, new int[] {0, 0}, new int[] {0, 0}, new char[] {'r', 'l'}))};
        static int curFNum = 1, hSFNum = 0; //current frame number, history start frame number
        static Socket serverSock = new Socket(SocketType.Dgram, ProtocolType.Udp);
        static Dictionary<IPEndPoint, int> playerIPs = new Dictionary<IPEndPoint, int>();



        //static TimeSpan[] pRTTs = new TimeSpan[] {TimeSpan.Zero, TimeSpan.Zero};
        //static Action<Socket, TimeSpan[], int> getRTT = (Socket sock, TimeSpan[] pRTTs, int i) =>
        //{
        //    sock.Send(new byte[1]);
        //    Console.WriteLine("[sent echo]");
        //    DateTime sendTime = DateTime.Now;
        //    sock.Receive(new byte[1]);
        //    Console.WriteLine("[recvd echo]");
        //    pRTTs[i] = DateTime.Now - sendTime;
        //    Console.WriteLine("[p" + (i+1) + " rtt: " + pRTTs[0].TotalMilliseconds +  ", comp: " + compensateLag + "]");
        //};
        //commands start with the player number. ex: "1Shoot"
        static GameState NextState(GameState state, string[] inputs, bool grid)
        {
            int speed = 5, blockDur = 20, blockCooldown = 300;
            if (grid) speed = 50;
            var nextState = new GameState(state);
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i][0] == '1')    //right
                {
                    nextState.positions[i] -= speed;
                    nextState.dirs[i] = 'r';
                }
                if (inputs[i][1] == '1')    //left
                {
                    nextState.positions[i] += speed;
                    nextState.dirs[i] = 'l';
                }
                if (inputs[i][2] == '1')    //block
                {
                    if (state.blockFrames[i] == -blockCooldown)
                    {
                        nextState.blockFrames[i] = blockDur;
                    }
                }
                if (state.blockFrames[i] > -blockCooldown)
                {
                    nextState.blockFrames[i] -= 1;
                }
                if (inputs[i][3] == '1')    //attack
                {
                    if (nextState.dirs[i] == 'r')
                    {
                        for (int j = 0; j < inputs.Length; j++)
                        {
                            if (j != i && state.blockFrames[j] <= 0)
                            {
                                if (state.positions[i] + 50 < state.positions[j] && state.positions[j] < state.positions[i] + 150)
                                {
                                    nextState.points[i] += 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < inputs.Length; j++)
                        {
                            if (j != i && state.blockFrames[j] <= 0)
                            {
                                if (state.positions[i] - 100 < state.positions[j] && state.positions[j] < state.positions[i])
                                {
                                    nextState.points[i] += 1;
                                }
                            }
                        }
                    }
                }   
            }
            return nextState;
        }
        /*
        async static void ManagePlayer(Socket sock, int player, DateTime time)
        {
            //get data
            sock.Receive(buffer);

            if (player == 1)
                await Task.Delay(simLag);

           string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0');
           if (data == "")
           {
                return; //a bit of a hack but whatever
           }
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

            int insertIndex = History.Count;
            while (time <= History[insertIndex - 1].Item1)
            {
                insertIndex--;
            }
            GameState prevGameState;
            if (insertIndex == History.Count)
            {
                Console.WriteLine("no redoing needed");
                prevGameState = History.Last().Item3;
                foreach (string command in commands)
                {
                    prevGameState = ExecuteCommand(player + command, prevGameState);
                    History.Add(new HistoryData(time, player + "" + command, prevGameState));
                }
            }
            else
            {
                Console.WriteLine("redoing needed");
                prevGameState = new GameState(History[insertIndex - 1].Item3);
                for (int i = insertIndex; i < insertIndex + commands.Length; i++)
                {
                    prevGameState = ExecuteCommand(player + commands[i - insertIndex], prevGameState);
                    History.Insert(i, new HistoryData(time, player + commands[0], prevGameState));
                }
                for (int i = insertIndex + commands.Length; i < History.Count; i++)
                {
                    string commandToRedo = History[i].Item2;
                    GameState updatedGameState = ExecuteCommand(commandToRedo, History[i - 1].Item3);
                    History[i] =  new HistoryData(time, commandToRedo, updatedGameState);
                }
            }

            //send update to both
            string gameStateString = JsonSerializer.Serialize(History.Last().Item3);
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
            foreach (HistoryData tgs in History.Skip(History.Count - 10))
            {
                printMsg += string.Format("{0}|{1}\n", tgs.Item1.ToString("mm.ss.fff"), JsonSerializer.Serialize(tgs.Item3));
            }
            Console.WriteLine(printMsg);
        }*/


        static void Rollback(string input, int fNum, int player)
        {
            if (history[fNum].inputs[player - 1] != input)
            {
                for (int i = fNum - hSFNum; i < history.Count; i++)
                {
                    Frame temp = history[i];
                    temp.inputs[player - 1] = input;
                    temp.state = NextState(history[i - 1].state, history[i].inputs, grid);
                    history[i] = temp;
                }
            }
        }


        static void GameLoop(object source, System.Timers.ElapsedEventArgs e)
        {
            List<string> packets = new List<string>();
            while (serverSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                serverSock.ReceiveFrom(buffer, ref clientEP); 
                packets.Append(playerIPs[(IPEndPoint)clientEP].ToString() + Encoding.Latin1.GetString(buffer));
            }
            //now each packet has (in this order): pnum, fnum, right, left, block, attack
            string[] latestInputs = new string[pCount];
            for (int i = 0; i < packets.Count; i++)
            {
                if (packets[i][1] == curFNum)
                {
                    latestInputs[int.Parse("" + packets[i][0]) - 1] = packets[i][2..];
                    packets.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < pCount; i++)
            {
                if (latestInputs[i] == null)
                {
                    latestInputs[i] = history.Last().inputs[i];
                }
            }
            history.Add(new Frame(latestInputs, NextState(history.Last().state, latestInputs, grid)));
            for (int i = 0; i < packets.Count; i++)
            {
                Rollback(packets[i][2..], int.Parse("" + packets[i][1]), int.Parse("" + packets[i][0]));
            }
            if (history.Count >= 200)
            {
                history.RemoveRange(0, 100);
            }
            string printMsg = "History:\n";
            foreach (var f in history.Skip(history.Count - 10))
            {
                printMsg += f.ToString() + "\n";
            }
            Console.WriteLine(printMsg);
        }


        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress sAddress = ipHost.AddressList[1];
            //address = IPAddress.Parse("172.16.2.167");
            sAddress = IPAddress.Parse("10.100.102.20");
            //Console.WriteLine(address);

            int cPort = 12345, sPort = 12346;
            serverSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            byte[] buffer = new byte[bufferSize];
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            serverSock.Bind(new IPEndPoint(sAddress, sPort));
            for (int i = 0; i < pCount; i++)
            {
                Console.WriteLine("Waiting for player " + (i+1));
                serverSock.ReceiveFrom(buffer, ref clientEP);
                string clientIPStr = clientEP.ToString();
                //clientIPStr = clientIPStr.Remove(clientIPStr.IndexOf(':'));
                Console.WriteLine(clientIPStr + " entered");
                //var test = clientEP.Serialize();
                //Console.WriteLine(test);
                playerIPs[new IPEndPoint(IPAddress.Parse(clientIPStr), cPort)] = i + 1;
            }
            foreach (var ip in playerIPs.Keys)
            {
                serverSock.SendTo(Encoding.Latin1.GetBytes(playerIPs[ip].ToString()), ip);
            }
            System.Timers.Timer gameLoop = new System.Timers.Timer(16);
            gameLoop.Elapsed += GameLoop;
            serverSock.Poll(-1, SelectMode.SelectRead);
            gameLoop.Start();
            Console.ReadKey();
            

            /*
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

            while (true)
            {
                //Thread.Sleep(200);
                // Get player commands and execute them
                //if (gameHistory.Count < 4)
                //{
                //    Console.WriteLine("while loop: " + DateTime.Now.ToString("mm.ss.fff"));
                //}
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
            }*/
        }
    }
}
