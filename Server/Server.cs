using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using System.Buffers.Binary;

namespace RealTimeProject
{
    internal class Server
    {
        const int bufferSize = 1024, pCount = 2, frameMS = 20;
        static bool grid = false, compensateLag = true;
        static int blockCooldown = 0, blockDuration = 40;
        //const int simLag = 0;

        static List<Frame> history = new List<Frame> ();
        static int curFNum = 0, hSFNum = 0; //current frame number, history start frame number
        static int[] playerLRFNum = new int[pCount]; //last recieved frame num for each player
        static DateTime gameStartTime;
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


        public struct ClientPacket
        {
            public byte[] data;
            public int player;

            public ClientPacket(int player, byte[] data)
            {
                this.data = data;
                this.player = player;
            }
        }
        public static GameState NextState(GameState state, string[] inputs, bool grid)
        {
            int speed = 5;
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
                        nextState.blockFrames[i] = blockDuration;
                    }
                }
                if (state.blockFrames[i] > -blockCooldown)
                {
                    nextState.blockFrames[i] -= 1;
                }
                if (inputs[i][3] == '1')    //attack
                {
                    nextState.attacks[i] = 1;
                    if (state.attacks[i] == 0)
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
                                    if (state.positions[i] - 100 < state.positions[j] + 50 && state.positions[j] + 50 < state.positions[i])
                                    {
                                        nextState.points[i] += 1;
                                    }
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


        static void Rollback(string input, DateTime time, int player)
        {
            for (int i = history.Count() - 1; i >= 0; i--)
            {
                if (history[i].startTime < time)
                {
                    if (history[i].inputs[player - 1] != input)
                    {
                        for (int j = i; j < history.Count; j++)
                        {
                            history[j].inputs[player - 1] = input;
                            history[j].state = NextState(history[j - 1].state, history[j].inputs, grid);
                        }
                    }
                    break;
                }
            }
        }


        static TimeSpan GameLoop(DateTime st)
        {
            DateTime frameStart = DateTime.Now;
            NBConsole.WriteLine(frameStart.ToString("mm.ss.fff"));
            curFNum++;
            //NBConsole.WriteLine("Started at " + st.Millisecond);

            NBConsole.WriteLine("Current frame num: " + curFNum); // get packets from players
            List<ClientPacket> packets = new List<ClientPacket>();
            while (serverSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[bufferSize];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                int bytesRecieved = serverSock.ReceiveFrom(buffer, ref clientEP);
                packets.Add(new ClientPacket(playerIPs[(IPEndPoint)clientEP], buffer[..bytesRecieved]));
                //NBConsole.WriteLine("Recieved " + Convert.ToHexString(packets.Last().data));
            }
            if (packets.Count == 0) { NBConsole.WriteLine("no user inputs recieved"); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets"); }
            //now each packet has (in this order): pnum, right, left, block, attack, timestamp

            string[] prevInputs = new string[pCount]; // add temp extrapolated state
            for (int i = 0; i < pCount; i++)
            {
                if (prevInputs[i] == null)
                {
                    prevInputs[i] = history.Last().inputs[i];
                }
            }
            history.Add(new Frame(frameStart, prevInputs, NextState(history.Last().state, prevInputs, grid)));

            int packetPlayer;
            DateTime packetTime;
            string packetInput;
            if (compensateLag)
            {
                for (int i = 0; i < packets.Count; i++) // apply inputs from packets, using general rollback
                {
                    packetPlayer = packets[i].player;
                    packetInput = Encoding.Latin1.GetString(packets[i].data[..4]);
                    packetTime = new DateTime(BinaryPrimitives.ReadInt64BigEndian(packets[i].data[4..]));
                    NBConsole.WriteLine("recieved inputs [" + packetInput + "], p" + packetPlayer + " from " + packetTime.ToString("mm.ss.fff") + " during frame that started at " + frameStart.ToString("mm.ss.fff"));
                    if (packetTime >= DateTime.Now)
                    {
                        throw new Exception("timestamp error");
                    }
                    Rollback(packetInput, packetTime, packetPlayer);
                }
            }
            else
            {
                string[] latestInputs = new string[pCount];
                prevInputs.CopyTo(latestInputs, 0);
                for (int i = 0; i < packets.Count; i++)
                {
                    packetPlayer = packets[i].player;
                    packetInput = Encoding.Latin1.GetString(packets[i].data[..4]);
                    packetTime = new DateTime(BinaryPrimitives.ReadInt64BigEndian(packets[i].data[4..]));
                    NBConsole.WriteLine("recieved inputs from " + packetTime.ToString("mm.ss.fff") + " during frame that started at " + frameStart.ToString("mm.ss.fff"));
                    latestInputs[packetPlayer - 1] = packetInput;
                    history.Last().inputs = latestInputs;
                    history.Last().state = NextState(history[history.Count - 1].state, latestInputs, grid);
                }
            }

            foreach (var ip in playerIPs.Keys) // send state to players
            {
                object[] sendData = new object[7];
                byte[] timeStamp = new byte[8];
                BinaryPrimitives.WriteInt64BigEndian(timeStamp, DateTime.Now.Ticks); ;
                sendData[0] = timeStamp;
                sendData[1] = history.Last().inputs;
                sendData[2] = history.Last().state.positions;
                sendData[3] = history.Last().state.points;
                sendData[4] = history.Last().state.blockFrames;
                sendData[5] = history.Last().state.dirs;
                sendData[6] = history.Last().state.attacks;
                //NBConsole.WriteLine(JsonSerializer.Serialize(sendData));
                serverSock.SendTo(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(sendData)), ip);
            }

            //old history deletion, maybe needed?
            if (history.Count >= 200)
            {
                history.RemoveRange(0, 100);
            }

            //print history
            string printMsg = "History:\n";
            foreach (var f in history.Skip(history.Count - 10))
            {
                printMsg += f.ToString() + "\n";
            }
            printMsg = printMsg.TrimEnd('\n');
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
            //sAddress = IPAddress.Parse("192.168.68.112");
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
                Console.WriteLine(clientEP.ToString() + " entered");
                playerIPs[new IPEndPoint(((IPEndPoint)clientEP).Address, ((IPEndPoint)clientEP).Port)] = i + 1;
            }
            foreach (var ip in playerIPs.Keys)
            {
                serverSock.SendTo(Encoding.Latin1.GetBytes(playerIPs[ip].ToString()), ip);
            }

            if (pCount == 1)
                history.Add(new Frame(DateTime.Now, new string[] { "0000" }, new GameState(new int[] { 0 }, new int[] { 0 }, new int[] { -blockCooldown }, new char[] { 'r' }, new int[] { 0 })));
            else if (pCount == 2)
                history.Add(new Frame(DateTime.Now, new string[] { "0000", "0000" }, new GameState(new int[] { 0, 100 }, new int[] { 0, 0 }, new int[] { -blockCooldown, -blockCooldown }, new char[] { 'r', 'l' }, new int[] { 0, 0 })));
            //serverSock.Poll(-1, SelectMode.SelectRead);
            gameStartTime = DateTime.Now;
            while (true)
            {
                TimeSpan duration = GameLoop(DateTime.Now);
                NBConsole.WriteLine("took " + duration.TotalMilliseconds + " ms");
                if (frameMS - duration.TotalMilliseconds > 0)
                {
                    NBConsole.WriteLine("Was fast, sleeping more");
                    Thread.Sleep(frameMS - (int)duration.TotalMilliseconds);
                }
                NBConsole.WriteLine("\n");
            }
        }
    }
}
