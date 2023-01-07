using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace RealTimeProject
{
    internal class Server
    {
        const int bufferSize = 1024, pCount = 2, frameMS = 17;
        static bool grid = false;
        static bool compensateLag = true;
        //const int simLag = 0;

        static List<Frame> history = new List<Frame> ();
        static int curFNum = 0, hSFNum = 0; //current frame number, history start frame number
        static int[] playerLRFNum = new int[pCount]; //last recieved frame num for each player
        static Socket serverSock = new Socket(SocketType.Dgram, ProtocolType.Udp);
        static Dictionary<IPEndPoint, int> playerIPs = new Dictionary<IPEndPoint, int>();


        public static class NBConsole
        {
            private static BlockingCollection<string> m_Queue = new BlockingCollection<string>();

            static NBConsole()
            {
                var thread = new Thread(
                  () =>
                  {
                      while (true) Console.WriteLine(m_Queue.Take());
                  });
                thread.IsBackground = true;
                thread.Start();
            }

            public static void WriteLine(string value)
            {
                m_Queue.Add(value);
            }
        }

        public static GameState NextState(GameState state, string[] inputs, bool grid)
        {
            int speed = 5, blockDur = 20, blockCooldown = 300;
            if (grid) speed = 50;
            var nextState = new GameState(state);
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i][0] == '1')    //right
                {
                    nextState.positions[i] += speed;
                    nextState.dirs[i] = 'r';
                }
                if (inputs[i][1] == '1')    //left
                {
                    nextState.positions[i] -= speed;
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
                    nextState.attacks[i] = 1;
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
                else
                {
                    nextState.attacks[i] = 0;
                }
            }
            return nextState;
        }


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


        static TimeSpan GameLoop(DateTime st)
        {
            curFNum++;
            //NBConsole.WriteLine("Started at " + st.Millisecond);
            List<string> packets = new List<string>();
            while (serverSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[bufferSize];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                serverSock.ReceiveFrom(buffer, ref clientEP);
                packets.Add(playerIPs[(IPEndPoint)clientEP].ToString() + Encoding.Latin1.GetString(buffer));
                NBConsole.WriteLine("Recieved " + packets.Last() + ", current frame " + curFNum + "| " + st.Millisecond);
            }
            if (packets.Count == 0) { NBConsole.WriteLine("no user inputs recieved" + "| " + st.Millisecond); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets" + "| " + st.Millisecond); }

            //now each packet has (in this order): pnum, right, left, block, attack, fNum
            string[] latestInputs = new string[pCount];
            int packetPlayer, packetFrame;
            string packetInput;
            for (int i = 0; i < packets.Count; i++)
            {
                packetPlayer = int.Parse("" + packets[i][0]);
                packetInput = packets[i][1..5];
                packetFrame = int.Parse(packets[i][5..]);
                if (packetFrame == curFNum)
                {
                    latestInputs[packetPlayer - 1] = packetInput;
                    packets.RemoveAt(i);
                    i--;
                    playerLRFNum[packetPlayer - 1] = curFNum;
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
                packetPlayer = int.Parse("" + packets[i][0]);
                packetInput = packets[i][1..5];
                packetFrame = int.Parse(packets[i][5..]);
                if (packetFrame < curFNum || true)
                {
                    NBConsole.WriteLine("Rollbacking" + "| " + st.Millisecond);
                    Rollback(packetInput, packetFrame, packetPlayer);
                }
                if (packetFrame > playerLRFNum[packetPlayer - 1])
                {
                    playerLRFNum[packetPlayer - 1] = packetFrame;
                }
            }

            foreach (var ip in playerIPs.Keys)
            {
                //serverSock.SendToAsync(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(history.Last())), SocketFlags.None, ip);
                object[] sendData = new object[7];
                int frameToSend = playerLRFNum[playerIPs[ip] - 1];
                sendData[0] = frameToSend;
                sendData[1] = history[frameToSend].inputs;
                sendData[2] = history[frameToSend].state.positions;
                sendData[3] = history[frameToSend].state.points;
                sendData[4] = history[frameToSend].state.blockFrames;
                sendData[5] = history[frameToSend].state.dirs;
                sendData[6] = history[frameToSend].state.attacks;
                Console.WriteLine(JsonSerializer.Serialize(sendData));
                serverSock.SendTo(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(sendData)), ip);
                //Console.WriteLine("last recieved fNum from player " + playerIPs[ip] + ", " + playerLRFNum[playerIPs[ip] - 1]);
            }

            //if (history.Count >= 200)
            //{
            //    history.RemoveRange(0, 100);
            //    hSFNum += 100;
            //}
            
            string printMsg = "History:\n";
            foreach (var f in history.Skip(history.Count - 20))
            {
                printMsg += f.ToString() + "\n";
            }
            TimeSpan duration = (DateTime.Now - st);
            //printMsg += "took" + duration.TotalMilliseconds + " ms" + "| " + st.Millisecond;
            NBConsole.WriteLine(printMsg);
            return duration;
        }


        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress sAddress = ipHost.AddressList[1];
            //address = IPAddress.Parse("172.16.2.167");
            sAddress = IPAddress.Parse("10.100.102.20");
            //Console.WriteLine(address);
            int sPort = 12345;
            serverSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            byte[] buffer = new byte[bufferSize];
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            serverSock.Bind(new IPEndPoint(sAddress, sPort));
            for (int i = 0; i < pCount; i++)
            {
                Console.WriteLine("Waiting for player " + (i + 1));
                serverSock.ReceiveFrom(buffer, ref clientEP);
                string clientIPStr = clientEP.ToString();
                //clientIPStr = clientIPStr.Remove(clientIPStr.IndexOf(':'));
                Console.WriteLine(clientIPStr + " entered");
                //var test = clientEP.Serialize();
                //Console.WriteLine(test);
                playerIPs[new IPEndPoint(((IPEndPoint)clientEP).Address, ((IPEndPoint)clientEP).Port)] = i + 1;
            }
            foreach (var ip in playerIPs.Keys)
            {
                serverSock.SendTo(Encoding.Latin1.GetBytes(playerIPs[ip].ToString()), ip);
            }

            if (pCount == 1)
                history.Add(new Frame(new string[] { "0000" }, new GameState(new int[] { 0 }, new int[] { 0 }, new int[] { -300 }, new char[] { 'r' }, new int[] { 0 })));
            else if (pCount == 2)
                history.Add(new Frame(new string[] { "0000", "0000" }, new GameState(new int[] { 0, 100 }, new int[] { 0, 0 }, new int[] { -300, -300 }, new char[] { 'r', 'l' }, new int[] { 0, 0 })));

            //serverSock.Poll(-1, SelectMode.SelectRead);
            while (true)
            {
                TimeSpan duration = GameLoop(DateTime.Now);
                NBConsole.WriteLine("took " + duration.TotalMilliseconds + " ms");
                if (frameMS - duration.TotalMilliseconds > 0)
                {
                    NBConsole.WriteLine("Was fast!");
                    Thread.Sleep(frameMS - (int)duration.TotalMilliseconds);
                }
            }
        }
    }
}
