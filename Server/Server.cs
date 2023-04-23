using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RealTimeProject
{
    public partial class Server : Form
    {
        static int bufferSize = 1024, pCount = 0;
        static bool compensateLag = true;
        static Server staticThis;
        static Dictionary<string, string> settings;
        static Socket serverSockUDP;
        static Dictionary<IPEndPoint, int> playerIPs = new Dictionary<IPEndPoint, int>();
        static List<Frame> history = new List<Frame>(200);
        //last recieved stamps for each player, 2 is for broken non lagcomp
        static DateTime[] playerLRS = new DateTime[pCount], playerLRS2 = new DateTime[pCount];
        static int curFNum = 0, hSFNum = 0; //current frame number, history start frame number

        bool keepPlayerJoining = true;
        private void InitLobby()
        {
            int maxPlayers = int.Parse(settings["maxPlayers"]);
            string ipStr = settings["serverIP"];
            int port = int.Parse(settings["serverPort"]);

            pCount = 0;
            history.Clear();
            playerIPs.Clear();
            keepPlayerJoining = true;
            InfoTextLabel.Text = "Opened lobby, waiting for players. Starts automatically at max or with button";
            PlayerListLabel.Text = "";

            serverSockUDP = CreateLocalSocket(ipStr, port);
            Task.Factory.StartNew(() => HandleTcpSockets(ipStr, port));
            Task.Factory.StartNew(() => ListenToUsers(maxPlayers));
        }

        static Socket CreateLocalSocket(string ipstr, int port)
        {
            Socket sock = new Socket(SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(new IPEndPoint(IPAddress.Parse(ipstr), port));
            return sock;
        }
        void HandleTcpSockets(string ipstr, int port)
        {
            Socket serverSockTcp = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSockTcp.Bind(new IPEndPoint(IPAddress.Parse(ipstr), port));
            serverSockTcp.Listen(5);
            List<Socket> readSocks = new List<Socket>() { serverSockTcp };
            List<Socket> writeSocks = new List<Socket>();
            List<Socket> checkReadSocks = new List<Socket>(), checkWriteSocks = new List<Socket>();
            byte[] buffer = new byte[1024];
            while (true)
            {
                checkReadSocks.Clear();
                checkWriteSocks.Clear();
                checkReadSocks.AddRange(readSocks);
                checkWriteSocks.AddRange(writeSocks);
                Console.WriteLine("Select");
                Socket.Select(checkReadSocks, checkWriteSocks, null, -1);
                foreach(Socket sock in checkReadSocks)
                {
                    if (sock == serverSockTcp)
                    {
                        Socket newClientSock = serverSockTcp.Accept();
                        Console.WriteLine("Connected to socket");
                        readSocks.Add(newClientSock);
                        writeSocks.Add(newClientSock);
                    }
                    else
                    {
                        int bytesRecieved = sock.Receive(buffer);
                        if (bytesRecieved == 0)
                        {
                            readSocks.Remove(sock);
                            writeSocks.Remove(sock);
                        }
                        else
                        {
                            sock.Send(TcpMessageResponse(buffer, bytesRecieved));
                        }
                    }
                }
                // TODO maybe add writing check and queue?

            }
        }

        byte[] TcpMessageResponse(byte[] data, int bytesRecieved)
        {
            byte[] msg = data[..bytesRecieved];
            ClientMessageType msgType = (ClientMessageType)msg[0];
            if (msgType == ClientMessageType.SignUp)
            {
                string[] uNamePass = JsonSerializer.Deserialize<string[]>(Encoding.Latin1.GetString(msg[1..]));
                if (DatabaseAccess.CheckIfUserNameExists(uNamePass[0]))
                {
                    return new byte[1] { (byte)ServerMessageType.Failure };
                }
                else
                {
                    DatabaseAccess.AddUser(new DatabaseAccess.User(uNamePass[0], uNamePass[1]));
                    return new byte[1] { (byte)ServerMessageType.Success };
                }
            }
            if (msgType == ClientMessageType.CheckSignIn)
            {
                string[] uNamePass = JsonSerializer.Deserialize<string[]>(Encoding.Latin1.GetString(msg[1..]));
                if (DatabaseAccess.CheckIfUserExists(uNamePass[0], uNamePass[1]))
                {
                    return new byte[1] { (byte)ServerMessageType.Success };
                }
                else
                {
                    return new byte[1] { (byte)ServerMessageType.Failure };
                }
            }
            if (msgType == ClientMessageType.GetMatchesWithUser)
            {
                string uName = Encoding.Latin1.GetString(msg[1..]);
                List<DatabaseAccess.Match> matchesWithUser = DatabaseAccess.GetMatchesWithUser(uName);
                string[][] MatchArr = new string[matchesWithUser.Count][];
                for (int i = 0; i < matchesWithUser.Count; i++)
                {
                    MatchArr[i] = matchesWithUser[i].GetProperyArray();
                }
                return Encoding.Latin1.GetBytes(JsonSerializer.Serialize(MatchArr));

            }
            return new byte[1] { (byte)ServerMessageType.Failure };
        }

        private void ListenToUsers(int maxPlayers)
        {
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            Task<SocketReceiveFromResult> playerJoinTask = serverSockUDP.ReceiveFromAsync(new byte[64], SocketFlags.None, clientEP);
            while (pCount < maxPlayers && keepPlayerJoining)
            {
                if (playerJoinTask.IsCompleted)
                {
                    Invoke(new Action(() => StartGameButton.Enabled = true));
                    SocketReceiveFromResult playerJoinTaskResult = playerJoinTask.Result;
                    clientEP = playerJoinTaskResult.RemoteEndPoint;
                    Invoke(new Action(() => PlayerListLabel.Text += "Player " + (pCount + 1) + ", " + clientEP.ToString() + " entered\n"));
                    playerIPs[new IPEndPoint(((IPEndPoint)clientEP).Address, ((IPEndPoint)clientEP).Port)] = pCount + 1;
                    pCount++;
                    playerJoinTask = serverSockUDP.ReceiveFromAsync(new byte[64], SocketFlags.None, clientEP);
                }
            }
            Invoke(new Action(() => InitGame()));
        }

        private void InitGame()
        {
            playerLRS = new DateTime[pCount];
            playerLRS2 = new DateTime[pCount];
            for (int i = 0; i < pCount; i++)
            {
                playerLRS[i] = DateTime.MinValue;
                playerLRS2[i] = DateTime.MinValue;
            }

            foreach (var ip in playerIPs.Keys)
            {
                serverSockUDP.SendTo(Encoding.Latin1.GetBytes(playerIPs[ip].ToString() + pCount.ToString()), ip);
            }

            history.Add(CreateInitFrame(pCount));
            InfoTextLabel.Text = "Game Running";
            StartGameButton.Enabled = false;
            GameLoopTimer.Enabled = true;
            ResetGameButton.Enabled = true;
            StopGameButton.Enabled = true;
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            DateTime frameStart = DateTime.Now;
            curFNum++;
            NBConsole.WriteLine("Frame start " + frameStart.ToString("mm.ss.fff") + ", Frame num: " + curFNum);

            List<ClientPacket> packets = new List<ClientPacket>();
            while (serverSockUDP.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[bufferSize];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                int bytesRecieved = serverSockUDP.ReceiveFrom(buffer, ref clientEP);
                if (playerIPs.ContainsKey((IPEndPoint)clientEP))
                    packets.Add(new ClientPacket(playerIPs[(IPEndPoint)clientEP], buffer[..bytesRecieved]));
            }
            if (packets.Count == 0) { NBConsole.WriteLine("no user inputs recieved"); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets"); }
            //now each packet has (in this order): pnum, right, left, block, attack, timestamp

            Input[] prevInputs = new Input[pCount]; // add temp extrapolated state
            for (int i = 0; i < pCount; i++)
            {
                prevInputs[i] = history.Last().Inputs[i];
            }
            history.Add(new Frame(frameStart, prevInputs, GameLogic.NextState(prevInputs, prevInputs, history.Last().State)));

            int packetPlayer;
            DateTime packetTime;
            Input packetInput;
            if (compensateLag)
            {
                for (int i = 0; i < packets.Count; i++) // apply inputs from packets, using lag compensation
                {
                    packetPlayer = packets[i].Player;
                    packetInput = (Input)packets[i].Data[0];
                    packetTime = new DateTime(BinaryPrimitives.ReadInt64BigEndian(packets[i].Data[1..]));
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
                Input[] latestInputs = new Input[pCount];
                prevInputs.CopyTo(latestInputs, 0);
                for (int i = 0; i < packets.Count; i++)
                {
                    packetPlayer = packets[i].Player;
                    packetInput = (Input)packets[i].Data[0];
                    packetTime = new DateTime(BinaryPrimitives.ReadInt64BigEndian(packets[i].Data[1..]));
                    playerLRS[packetPlayer - 1] = frameStart;
                    if (packetTime > playerLRS2[packetPlayer - 1])
                    {
                        playerLRS2[packetPlayer - 1] = packetTime;
                    }
                    NBConsole.WriteLine("recieved inputs [" + packetInput + "], p" + packetPlayer + " from " + packetTime.ToString("mm.ss.fff") + " during frame that started at " + frameStart.ToString("mm.ss.fff"));

                    latestInputs[packetPlayer - 1] = packetInput;
                }
                history.Last().Inputs = latestInputs;
                if (history.Count > 4)
                    history.Last().State = GameLogic.NextState(history[history.Count - 2].Inputs, latestInputs, history[history.Count - 2].State);
            }

            foreach (var ip in playerIPs.Keys) // send state to players
            {
                int thisPlayer = playerIPs[ip];
                if (playerLRS[thisPlayer - 1] != DateTime.MinValue)
                {
                    DateTime saveNow = DateTime.Now;
                    int startI = 1;
                    for (int i = history.Count - 1; i >= 0; i--)
                    {
                        //NBConsole.WriteLine("Checking whether player " + thisPlayer + " last stamp  " + playerLRS[thisPlayer - 1].ToString("mm.ss.fff") + " was in frame " + history[i].startTime.ToString("mm.ss.fff"));
                        if (history[i].StartTime <= playerLRS[thisPlayer - 1])
                        {
                            startI = i + 1;
                            break;
                        }
                    }
                    NBConsole.WriteLine("sending player" + thisPlayer + " " + (history.Count - startI) + " enemy inputs to catch up");
                    Input[][] enemyInputs = new Input[pCount - 1][];
                    for (int j = 0; j < pCount; j++)
                    {
                        if (j != thisPlayer - 1)
                        {
                            Input[] oneEnemyInput = new Input[history.Count - startI];
                            for (int i = startI; i < history.Count; i++)
                            {
                                oneEnemyInput[i - startI] = history[i].Inputs[j];
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
                        sendFrame.StartTime = playerLRS2[thisPlayer - 1];
                    }

                    Console.WriteLine("sending: " + new ServerGamePacket(saveNow, sendFrame, enemyInputs));
                    serverSockUDP.SendTo(ServerGamePacket.Serialize(saveNow, sendFrame, enemyInputs, pCount), ip);
                }
            }

            //print history
            string printMsg = "History:\n";
            foreach (var f in history.Skip(history.Count - 10))
            {
                printMsg += f.ToString() + "\n";
            }
            NBConsole.WriteLine(printMsg);
            TimeSpan duration = (DateTime.Now - frameStart);
            printMsg += "took " + duration.TotalMilliseconds + " ms\n";
        }

        static Frame CreateInitFrame(int playerCount)
        {
            if (playerCount == 1)
                return new Frame(DateTime.MinValue, new Input[] { Input.None }, GameLogic.InitialState(1));
            if (playerCount == 2)
                return new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None }, GameLogic.InitialState(2));
            if (playerCount == 3)
                return new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None, Input.None }, GameLogic.InitialState(3));
            return new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None, Input.None, Input.None }, GameLogic.InitialState(4));
        }

        static void Rollback(Input input, DateTime time, int player)
        {
            for (int i = history.Count() - 1; i >= 0; i--)
            {
                if (history[i].StartTime < time)
                {
                    if (history[i].Inputs[player - 1] != input)
                    {
                        for (int j = i; j < history.Count; j++)
                        {
                            history[j].Inputs[player - 1] = input;
                            history[j].State = GameLogic.NextState(history[j - 1].Inputs, history[j].Inputs, history[j - 1].State);
                        }
                    }
                    break;
                }
            }
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            keepPlayerJoining = false;
        }
        private void ResetGameButton_Click(object sender, EventArgs e)
        {
            history[history.Count - 1] = CreateInitFrame(pCount);
        }

        private void StopGameButton_Click(object sender, EventArgs e)
        {
            GameLoopTimer.Enabled = false;
            Thread.Sleep(15);

            foreach (var ip in playerIPs.Keys)
            {
                serverSockUDP.SendTo(Encoding.Latin1.GetBytes("a"), ip);
            }
            serverSockUDP.Close();

            ResetGameButton.Enabled = false;
            StopGameButton.Enabled = false;
            InitLobby();
        }

        

        
        public Server()
        {
            InitializeComponent();
            staticThis = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.GetFullPath("serverSettings.txt")));
            InitLobby();
        }
    }
}