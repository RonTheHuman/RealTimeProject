using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace RealTimeProject
{
    internal class ServerFuncs
    {
        public static List<Frame> history = new List<Frame>(200);
        public static ConcurrentDictionary<IPEndPoint, LobbyPlayer> lobbyPlayerDict = new ConcurrentDictionary<IPEndPoint, LobbyPlayer>();
        public static int currentFNum;
        
        public static Form UI;
        public static Action<string> OnLobbyUpdate;
        public static Action OnInitGame;
        public static Action OnEndGame;
        public static int levelLayout;
        public static byte frameMS;

        static Dictionary<string, string> settings;
        static int pCount = 0;
        static DateTime[] playerLRS, playerLRS2;
        static int bufferSize = 1024;
        static bool compensateLag = true;
        static TimeSpan gameLength;
        static DateTime gameStartTime;

        static Socket serverSockUdp;

        static bool gameRunning;

        static public void InitServer()
        {
            settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.GetFullPath("serverSettings.txt")));
            Task.Factory.StartNew(() => HandleTcpSockets(settings["serverIP"], int.Parse(settings["serverPort"])));
            InitLobby();
        }
        static public void InitLobby()
        {
            pCount = 0;
            history.Clear();
            lobbyPlayerDict.Clear();
            gameRunning = false;
            serverSockUdp = CreateLocalSocket(settings["serverIP"], int.Parse(settings["serverPort"]));
        }
        static public void InitGame()
        {
            gameRunning = true;
            playerLRS = new DateTime[pCount];
            playerLRS2 = new DateTime[pCount];
            for (int i = 0; i < pCount; i++)
            {
                playerLRS[i] = DateTime.MinValue;
                playerLRS2[i] = DateTime.MinValue;
            }
            foreach (LobbyPlayer lp in lobbyPlayerDict.Values)
            {
                if (!lp.Disconnected)
                {
                    lp.Sock.Send(Encoding.Latin1.GetBytes(lp.Number.ToString() + pCount.ToString() + levelLayout));
                }
            }
            history.Add(CreateInitFrame(pCount));
            gameStartTime = DateTime.Now;
            UI.Invoke(OnInitGame);
        }
        static public void ResetGame()
        {
            history[history.Count - 1] = CreateInitFrame(pCount);
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
        static public void OnTimerTick()
        {
            DateTime frameStart = DateTime.Now;
            currentFNum++;
            NBConsole.WriteLine("Frame start " + frameStart.ToString("mm.ss.fff") + ", Frame num: " + currentFNum);

            List<ClientPacket> packets = new List<ClientPacket>();
            while (serverSockUdp.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[bufferSize];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    int bytesRecieved = serverSockUdp.ReceiveFrom(buffer, ref clientEP);
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
                    serverSockUdp.SendTo(sendPacket.Serialize(pCount), ip);
                }
            }

            //print history
            string printMsg = "History:\n";
            foreach (var f in history.Skip(history.Count - 10))
            {
                printMsg += f.ToString() + "\n";
            }
            TimeSpan duration = (DateTime.Now - frameStart);
            printMsg += "took " + duration.TotalMilliseconds + " ms\n";
            NBConsole.WriteLine(printMsg);
        }
        public static void EndGame(int winner)
        {
            OnEndGame();
            gameLength = DateTime.Now - gameStartTime;
            Thread.Sleep(15);
            string playerString = "", winnerString = "Tie";
            foreach (var lp in lobbyPlayerDict.Values)
            {
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
                    byte[] toSend = new byte[1 + winnerString.Length];
                    toSend[0] = (byte)ServerMessageType.GameEnd;
                    Encoding.Latin1.GetBytes(winnerString).CopyTo(toSend, 1);
                    lp.Sock.Send(toSend);
                }
            }
            playerString = playerString.Substring(0, playerString.Length - 2);
            DatabaseAccess.AddMatch(new Match(gameStartTime.ToString("d/M/yyyy HH:mm"), playerString, winnerString, MinutesToString(gameLength.TotalMinutes)));
            serverSockUdp.Close();
            Console.WriteLine("Got to init lobby");
            InitLobby();
        }

        static void HandleTcpSockets(string ipstr, int port)
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
                foreach (Socket sock in checkErrorSocks)
                {
                    Console.WriteLine("" + sock.RemoteEndPoint + " crashed");
                    readSocks.Remove(sock);
                    errorSocks.Remove(sock);
                    if (lobbyPlayerDict.ContainsKey((IPEndPoint)sock.RemoteEndPoint))
                    {
                        lobbyPlayerDict[(IPEndPoint)sock.RemoteEndPoint].Disconnected = true;
                    }
                }
                foreach (Socket sock in checkReadSocks)
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
                        try
                        {
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
            }
        }
        static void TcpMessageResponse(byte[] data, int bytesRecieved, Socket pSock)
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
                    UI.Invoke(OnLobbyUpdate, new string[] { "Player " + (pCount + 1) + ", " + pSock.RemoteEndPoint.ToString() + " entered\n" });
                    pCount += 1;
                    if (pCount == int.Parse(settings["maxPlayers"]))
                    {
                        InitGame();
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
                UI.Invoke(OnLobbyUpdate, new string[] { "Player " + (pCount + 1) + ", " + pSock.RemoteEndPoint.ToString() + " left\n" });
                pSock.Send(new byte[1] { (byte)ServerMessageType.Failure });
            }
        }

        static bool IsUserNameInLobby(string uName, out IPEndPoint ipWithName)
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
        static Socket CreateLocalSocket(string ipstr, int port)
        {
            Socket sock = new Socket(SocketType.Dgram, ProtocolType.Udp);
            sock.Bind(new IPEndPoint(IPAddress.Parse(ipstr), port));
            return sock;
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
        static string MinutesToString(double totalMinutes)
        {
            int mins = (int)totalMinutes;
            int secs = (int)((totalMinutes - mins) * 60);
            return "" + mins + ":" + secs;
        }
    }
}
