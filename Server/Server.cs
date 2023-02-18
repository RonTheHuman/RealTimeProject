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
        static int bufferSize = 1024, pCount = 2;
        static bool grid = false, compensateLag = true;

        static DateTime gameStartTime;
        static List<Frame> history = new List<Frame>(200);
        static int curFNum = 0, hSFNum = 0; //current frame number, history start frame number
        static DateTime[] playerLRS = new DateTime[pCount], playerLRS2 = new DateTime[pCount]; //last recieved stamp for each player
        static int[] playerLRSIndex = new int[pCount];
        static Dictionary<IPEndPoint, int> playerIPs = new Dictionary<IPEndPoint, int>();
        static Socket serverSock = new Socket(SocketType.Dgram, ProtocolType.Udp);


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
                            history[j].state = GameState.NextState(history[j - 1].state, history[j].inputs, grid);
                        }
                    }
                    break;
                }
            }
        }


        static byte[] Serialize(byte[] timeStamp, Frame state, string[][] enemyInputs)
        {
            object[] sendData = new object[7 + pCount];
            sendData[0] = timeStamp;
            byte[] frameTime = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(frameTime, state.startTime.Ticks);
            sendData[1] = frameTime;
            sendData[2] = state.inputs;
            sendData[3] = state.state.positions;
            sendData[4] = state.state.points;
            sendData[5] = state.state.blockFrames;
            sendData[6] = state.state.dirs;
            sendData[7] = state.state.attacks;
            for (int i = 0; i < pCount - 1; i++)
            {
                sendData[8 + i] = enemyInputs[i];
            }
            string jsonString = JsonSerializer.Serialize(sendData);
            NBConsole.WriteLine(jsonString);
            return Encoding.Latin1.GetBytes(jsonString);
        }


        static TimeSpan GameLoop(DateTime st)
        {
            DateTime frameStart = DateTime.Now;
            curFNum++;

            NBConsole.WriteLine("Frame start " + frameStart.ToString("mm.ss.fff") + ", Frame num: " + curFNum);

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
            history.Add(new Frame(frameStart, prevInputs, GameState.NextState(history.Last().state, prevInputs, grid)));

            int packetPlayer;
            DateTime packetTime;
            string packetInput;
            if (compensateLag)
            {
                for (int i = 0; i < packets.Count; i++) // apply inputs from packets, using lag compensation
                {
                    packetPlayer = packets[i].player;
                    packetInput = Encoding.Latin1.GetString(packets[i].data[..4]);
                    packetTime = new DateTime(BinaryPrimitives.ReadInt64BigEndian(packets[i].data[4..]));
                    if (packetTime > playerLRS[packetPlayer - 1])
                    {
                        playerLRS[packetPlayer - 1] = packetTime;
                    }
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
                    playerLRS[packetPlayer - 1] = frameStart;
                    if (packetTime > playerLRS2[packetPlayer - 1])
                    {
                        playerLRS2[packetPlayer - 1] = packetTime;
                    }
                    NBConsole.WriteLine("recieved inputs [" + packetInput + "], p" + packetPlayer + " from " + packetTime.ToString("mm.ss.fff") + " during frame that started at " + frameStart.ToString("mm.ss.fff"));

                    latestInputs[packetPlayer - 1] = packetInput;
                }
                history.Last().inputs = latestInputs;
                history.Last().state = GameState.NextState(history[history.Count - 2].state, latestInputs, grid);
            }

            foreach (var ip in playerIPs.Keys) // send state to players
            {
                int thisPlayer = playerIPs[ip];
                if (playerLRS[thisPlayer - 1] != DateTime.MinValue)
                {
                    byte[] timeStamp = new byte[8];
                    BinaryPrimitives.WriteInt64BigEndian(timeStamp, DateTime.Now.Ticks);
                    int startI = 1;
                    for (int i = history.Count - 1; i >= 0; i--)
                    {
                        //NBConsole.WriteLine("Checking whether player " + thisPlayer + " last stamp  " + playerLRS[thisPlayer - 1].ToString("mm.ss.fff") + " was in frame " + history[i].startTime.ToString("mm.ss.fff"));
                        if (history[i].startTime <= playerLRS[thisPlayer - 1])
                        {
                             startI = i + 1;
                             break;
                        }
                    }
                    NBConsole.WriteLine("sending player" + thisPlayer + " " + (history.Count - startI) + " enemy inputs to catch up");
                    string[][] enemyInputs = new string[pCount - 1][];
                    for (int j = 0; j < pCount; j++)
                    {
                        if (j != thisPlayer - 1)
                        {
                            string[] oneEnemyInput = new string[history.Count - startI];
                            for (int i = startI; i < history.Count; i++)
                            {
                                oneEnemyInput[i - startI] = history[i].inputs[j];
                            }
                            if (j < thisPlayer)
                            {
                                enemyInputs[j] = oneEnemyInput;
                            }
                            else
                            {
                                enemyInputs[j - 1] = oneEnemyInput;
                            }
                        }
                    }
                    Frame sendFrame = history[startI - 1];
                    if (!compensateLag)
                    {
                        sendFrame = new Frame(sendFrame);
                        sendFrame.startTime = playerLRS2[thisPlayer - 1];
                    }
                    serverSock.SendTo(Serialize(timeStamp, sendFrame, enemyInputs), ip);
                }
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
            IPAddress autoAdress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            string[] adresses = new string[3] { "172.16.2.167", "10.100.102.20", "192.168.68.112" };
            IPAddress sAddress = IPAddress.Parse(adresses[1]);
            int sPort = 12345;
            byte[] buffer = new byte[bufferSize];
            serverSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            serverSock.Bind(new IPEndPoint(sAddress, sPort));

            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            for (int i = 0; i < pCount; i++)
            {
                Console.WriteLine("Waiting for player " + (i + 1));
                serverSock.ReceiveFrom(buffer, ref clientEP);
                Console.WriteLine(clientEP.ToString() + " entered");
                playerIPs[new IPEndPoint(((IPEndPoint)clientEP).Address, ((IPEndPoint)clientEP).Port)] = i + 1;
                playerLRS[i] = DateTime.MinValue;
                playerLRS2[i] = DateTime.MinValue;
            }

            foreach (var ip in playerIPs.Keys)
            {
                serverSock.SendTo(Encoding.Latin1.GetBytes(playerIPs[ip].ToString()), ip);
            }

            if (pCount == 1)
                history.Add(new Frame(DateTime.MinValue, new string[] { "0000" }, GameState.InitialState(1)));
            else if (pCount == 2)
                history.Add(new Frame(DateTime.MinValue, new string[] { "0000", "0000" }, GameState.InitialState(2)));

            gameStartTime = DateTime.Now;
            while (true)
            {
                TimeSpan duration = GameLoop(DateTime.Now);
                NBConsole.WriteLine("took " + duration.TotalMilliseconds + " ms");
                NBConsole.WriteLine("\n");
            }
        }
    }
}
