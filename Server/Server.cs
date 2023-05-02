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
        static int bufferSize = 1024, pCount = 0, levelLayout;
        static bool compensateLag = true, gameRunning = false;
        static Server staticThis;
        static Dictionary<string, string> settings;
        static Socket serverSockUDP;
        static ConcurrentDictionary<IPEndPoint, LobbyPlayer> lobbyPlayerDict = new ConcurrentDictionary<IPEndPoint, LobbyPlayer>();
        static List<Frame> history = new List<Frame>(200);
        static byte frameMS = 15;
        //last recieved stamps for each player, 2 is for broken non lagcomp
        static DateTime[] playerLRS = new DateTime[pCount], playerLRS2 = new DateTime[pCount];
        static DateTime gameStartTime;
        static TimeSpan gameLength;
        static int curFNum = 0, hSFNum = 0; //current frame number, history start frame number

        private void InitLobby()
        {
            pCount = 0;
            history.Clear();
            lobbyPlayerDict.Clear();
            gameRunning = false;
            InfoTextLabel.Text = "Opened lobby, waiting for players. Starts automatically at max or with button";
            PlayerListLabel.Text = "";

            serverSockUDP = CreateLocalSocket(settings["serverIP"], int.Parse(settings["serverPort"]));
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
            List<Socket> errorSocks = new List<Socket>();
            List<Socket> checkReadSocks = new List<Socket>(), checkErrorSocks = new List<Socket>();
            byte[] buffer = new byte[1024];
            while (true)
            {
                checkReadSocks.Clear();
                checkErrorSocks.Clear();
                checkReadSocks.AddRange(readSocks);
                checkErrorSocks.AddRange(errorSocks);
                Console.WriteLine("Select");
                Socket.Select(checkReadSocks, null, checkErrorSocks, -1);
                foreach(Socket sock in checkErrorSocks)
                {
                    Console.WriteLine("" + sock.RemoteEndPoint + " crashed");
                    readSocks.Remove(sock);
                    errorSocks.Remove(sock);
                    if (lobbyPlayerDict.ContainsKey((IPEndPoint)sock.RemoteEndPoint))
                    {
                        lobbyPlayerDict[(IPEndPoint)sock.RemoteEndPoint].Disconnected = true;
                    }
                }
                foreach(Socket sock in checkReadSocks)
                {
                    if (sock == serverSockTcp)
                    {
                        Socket newClientSock = serverSockTcp.Accept();
                        Console.WriteLine("Connected to socket");
                        readSocks.Add(newClientSock);
                        errorSocks.Add(newClientSock);
                    }
                    else
                    {
                        try { 
                            int bytesRecieved = sock.Receive(buffer);
                            if (bytesRecieved == 0)
                            {
                                Console.WriteLine("" + sock.RemoteEndPoint + " disconnected");
                                readSocks.Remove(sock);
                                errorSocks.Remove(sock);
                                if (lobbyPlayerDict.ContainsKey((IPEndPoint)sock.RemoteEndPoint))
                                {
                                    lobbyPlayerDict[(IPEndPoint)sock.RemoteEndPoint].Disconnected = true;
                                }
                            }
                            else
                            {
                                TcpMessageResponse(buffer, bytesRecieved, sock);
                            }
                        }
                        catch 
                        {
                            Console.WriteLine("" + sock.RemoteEndPoint + " crashed");
                            readSocks.Remove(sock);
                            errorSocks.Remove(sock);
                            if (lobbyPlayerDict.ContainsKey((IPEndPoint)sock.RemoteEndPoint))
                            {
                                lobbyPlayerDict[(IPEndPoint)sock.RemoteEndPoint].Disconnected = true;
                            }
                        }
                        
                    }
                }
                // TODO maybe add writing check and queue?
            }
        }

        void TcpMessageResponse(byte[] data, int bytesRecieved, Socket pSock)
        {
            byte[] msg = data[..bytesRecieved];
            ClientMessageType msgType = (ClientMessageType)msg[0];
            if (msgType == ClientMessageType.SignUp)
            {
                string[] uNamePass = JsonSerializer.Deserialize<string[]>(Encoding.Latin1.GetString(msg[1..]));
                if (DatabaseAccess.CheckIfUserNameExists(uNamePass[0]))
                {
                    pSock.Send(new byte[1] { (byte)ServerMessageType.Failure });
                }
                else
                {
                    DatabaseAccess.AddUser(new User(uNamePass[0], uNamePass[1]));
                    pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                }
            }
            else if (msgType == ClientMessageType.CheckSignIn)
            {
                string[] uNamePass = JsonSerializer.Deserialize<string[]>(Encoding.Latin1.GetString(msg[1..]));
                if (DatabaseAccess.CheckIfUserExists(uNamePass[0], uNamePass[1]))
                {
                    pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                }
                else
                {
                    pSock.Send(new byte[1] { (byte)ServerMessageType.Failure });
                }
            }
            else if (msgType == ClientMessageType.GetMatchesWithUser)
            {
                string uName = Encoding.Latin1.GetString(msg[1..]);
                List<Match> matchesWithUser = DatabaseAccess.GetMatchesWithUser(uName);
                string[][] MatchArr = new string[matchesWithUser.Count][];
                for (int i = 0; i < matchesWithUser.Count; i++)
                {
                    MatchArr[i] = matchesWithUser[i].GetProperyArray();
                }
                pSock.Send(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(matchesWithUser) + "|"));

            }
            else if (msgType == ClientMessageType.JoinLobby)
            {
                string uName = Encoding.Latin1.GetString(msg[1..]);
                if (!gameRunning)
                {
                    lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint] = new LobbyPlayer(uName, pCount + 1, pSock);
                    Invoke(() => StartGameButton.Enabled = true);
                    Invoke(() => PlayerListLabel.Text += "Player " + (pCount + 1) + ", " + pSock.RemoteEndPoint.ToString() + " entered\n");
                    pCount += 1;
                    if (pCount == int.Parse(settings["maxPlayers"]))
                    {
                        Invoke(() => InitGame());
                    }
                    pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                }
                else
                {
                    if (lobbyPlayerDict.ContainsKey((IPEndPoint)pSock.RemoteEndPoint))
                    {
                        if (lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].UName == uName)
                        {
                            lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Sock = pSock;
                            lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Disconnected = false;
                            pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                            pSock.Send(Encoding.Latin1.GetBytes(lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Number.ToString() + pCount.ToString() + levelLayout));
                        }
                    }
                    else if (IsUserNameInLobby(uName, out IPEndPoint ipWithName))
                    {
                        lobbyPlayerDict.Remove(ipWithName, out LobbyPlayer removedPlayer);
                        removedPlayer.Sock = pSock;
                        lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Disconnected = false;
                        lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint] = removedPlayer;
                        pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                        pSock.Send(Encoding.Latin1.GetBytes(lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Number.ToString() + pCount.ToString() + levelLayout));
                    }
                    else
                        pSock.Send(new byte[1] { (byte)ServerMessageType.Failure });
                }

            }
            else if (msgType == ClientMessageType.ExitLobby)
            {
                lobbyPlayerDict.Remove((IPEndPoint)pSock.RemoteEndPoint, out LobbyPlayer removedPlayer);
                pCount -= 1;
                if (pCount == 0)
                {
                    Invoke(() => StartGameButton.Enabled = false);
                }
                pSock.Send(new byte[1] { (byte)ServerMessageType.Failure });
            }
        }

        private bool IsUserNameInLobby(string uName, out IPEndPoint ipWithName)
        {
            foreach (var ip in lobbyPlayerDict.Keys)
            {
                if (lobbyPlayerDict[ip].UName == uName)
                {
                    ipWithName = ip;
                    return true;
                }
            }
            ipWithName = null;
            return false;
        }

        private void InitGame()
        {
            gameRunning = true;
            playerLRS = new DateTime[pCount];
            playerLRS2 = new DateTime[pCount];
            for (int i = 0; i < pCount; i++)
            {
                playerLRS[i] = DateTime.MinValue;
                playerLRS2[i] = DateTime.MinValue;
            }

            levelLayout = int.Parse(LevelLayoutComboBox.Text);
            foreach (LobbyPlayer lp in lobbyPlayerDict.Values)
            {
                Console.WriteLine(LevelLayoutComboBox.Text);
                if (!lp.Disconnected)
                {
                    lp.Sock.Send(Encoding.Latin1.GetBytes(lp.Number.ToString() + pCount.ToString() + levelLayout));
                }
            }

            history.Add(CreateInitFrame(pCount));
            InfoTextLabel.Text = "Game Running";
            StartGameButton.Enabled = false;
            GameLoopTimer.Enabled = true;
            ResetGameButton.Enabled = true;
            StopGameButton.Enabled = true;
            gameStartTime = DateTime.Now;
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
                try
                {
                    int bytesRecieved = serverSockUDP.ReceiveFrom(buffer, ref clientEP);
                    if (lobbyPlayerDict.ContainsKey((IPEndPoint)clientEP))
                        packets.Add(new ClientPacket(lobbyPlayerDict[(IPEndPoint)clientEP].Number, buffer[..bytesRecieved]));
                }
                catch (Exception ex)
                {
                    if (ex is SocketException)
                    {
                        Console.WriteLine("Client Disconnected, stopped stupid error");
                    }
                }
            }
            if (packets.Count == 0) { NBConsole.WriteLine("no user inputs recieved"); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets"); }
            //now each packet has (in this order): pnum, right, left, block, attack, timestamp

            Input[] prevInputs = new Input[pCount]; // add temp extrapolated state
            for (int i = 0; i < pCount; i++)
            {
                prevInputs[i] = history.Last().Inputs[i];
            }
            history.Add(new Frame(frameStart, prevInputs, GameLogic.NextState(prevInputs, prevInputs, history.Last().State, levelLayout)));

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
                    history.Last().State = GameLogic.NextState(history[history.Count - 2].Inputs, latestInputs, history[history.Count - 2].State, levelLayout);
            }

            //check if the game is over even for the laggiest player (so there's is no lag compensation guessing involved)
            DateTime oldestStamp = playerLRS.Min();
            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (history[i].StartTime <= oldestStamp)
                {
                    int winner = 0;
                    if (GameLogic.IsGameOver(history[i].State, ref winner))
                    {
                        EndGame(winner);
                        return;
                    }
                }
            }

            foreach (var ip in lobbyPlayerDict.Keys) // send state to players
            {
                int thisPlayer = lobbyPlayerDict[ip].Number;
                if (DateTime.Now - playerLRS[thisPlayer - 1] > TimeSpan.FromSeconds(2))
                {
                    NBConsole.WriteLine("Disconnected for time reasons");
                    lobbyPlayerDict[ip].Disconnected = true;
                }
                else
                {
                    lobbyPlayerDict[ip].Disconnected = false;
                }
                if (playerLRS[thisPlayer - 1] != DateTime.MinValue && !lobbyPlayerDict[ip].Disconnected)
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
                    ServerGamePacket sendPacket = new ServerGamePacket(saveNow, sendFrame, enemyInputs, frameMS);
                    Console.WriteLine("sending: " + sendPacket);
                    serverSockUDP.SendTo(sendPacket.Serialize(pCount), ip);
                }
            }

            //print history
            string printMsg = "History:\n";
            foreach (var f in history.Skip(history.Count - 10))
            {
                printMsg += f.ToString() + "\n";
            }
            //NBConsole.WriteLine(printMsg);
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
                            history[j].State = GameLogic.NextState(history[j - 1].Inputs, history[j].Inputs, history[j - 1].State, levelLayout);
                        }
                    }
                    break;
                }
            }
        }

        private void StartGameButton_Click(object sender, EventArgs e)
        {
            InitGame();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            frameMS = (byte)numericUpDown1.Value;
            GameLoopTimer.Interval = frameMS;
        }

        private void ResetGameButton_Click(object sender, EventArgs e)
        {
            history[history.Count - 1] = CreateInitFrame(pCount);
        }

        private void StopGameButton_Click(object sender, EventArgs e)
        {
            EndGame(0);
        }

        private void EndGame(int winner)
        {
            gameLength = DateTime.Now - gameStartTime;
            GameLoopTimer.Enabled = false;
            Thread.Sleep(15);
            string playerString = "", winnerString = "Tie";
            foreach (var lp in lobbyPlayerDict.Values)
            {
                Console.WriteLine("Got to loop " + lp.Disconnected);
                if (lp.UName == "guest")
                {
                    playerString += lp.UName + lp.Number + ", ";
                    if (lp.Number == winner)
                        winnerString = lp.UName + lp.Number;
                }
                else
                {
                    playerString += lp.UName + ", ";
                    if (lp.Number == winner)
                        winnerString = lp.UName;
                }
            }
            foreach (var lp in lobbyPlayerDict.Values)
            {
                if (!lp.Disconnected)
                {
                    Console.WriteLine("Sent end message");
                    byte[] toSend = new byte[1 + winnerString.Length];
                    toSend[0] = (byte)ServerMessageType.GameEnd;
                    Encoding.Latin1.GetBytes(winnerString).CopyTo(toSend, 1);
                    lp.Sock.Send(toSend);
                }
            }
            playerString = playerString.Substring(0, playerString.Length - 2);
            Console.WriteLine("Got To database");
            DatabaseAccess.AddMatch(new Match(gameStartTime.ToString("d/M/yyyy HH:mm"), playerString, winnerString, MinutesToString(gameLength.TotalMinutes)));
            serverSockUDP.Close();
            ResetGameButton.Enabled = false;
            StopGameButton.Enabled = false;
            Console.WriteLine("Got to init lobby");
            InitLobby();
        }
        
        string MinutesToString(double totalMinutes)
        {
            int mins = (int)totalMinutes;
            int secs = (int)((totalMinutes - mins)*60);
            return "" + mins + ":" + secs;
        }

        public Server()
        {
            InitializeComponent();
            staticThis = this;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.GetFullPath("serverSettings.txt")));
            Task.Factory.StartNew(() => HandleTcpSockets(settings["serverIP"], int.Parse(settings["serverPort"])));
            InitLobby();
        }
    }
}