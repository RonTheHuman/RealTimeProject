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
        public static int curFNum;
        
        public static Form UI;
        public static Action OnInitGame;
        public static Action OnEndGame;
        public static Action<string> OnLobbyUpdate;
        public static int levelLayout;
        public static byte frameMS;

        static Dictionary<string, string> settings;
        static int pCount = 0;
        static DateTime[] playerLRS, playerLRS2, echoSendTime;
        static double[] echoDelayMS;
        static bool[] echoWaiting;
        static int bufferSize = 1024;
        static bool compensateLag = true;
        static TimeSpan gameLength;
        static DateTime gameStartTime;

        static public void InitServer()
        {
            settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.GetFullPath("serverSettings.txt")));
            Task.Factory.StartNew(() => ServerSockFuncs.HandleTcpSockets(settings["serverIP"], int.Parse(settings["serverPort"])));
            ServerSockFuncs.CreateUdpSocket(settings["serverIP"], int.Parse(settings["serverPort"]));
            InitLobby();
        }

        static public void InitLobby()
        {
            pCount = 0;
            history.Clear();
            ServerSockFuncs.lobbyPlayerDict.Clear();
            ServerSockFuncs.gameRunning = false;
        }
        // Initializes arrays needed for running the game, and sends start game messages to all players in the lobby.
        static public void InitGame()
        {
            ServerSockFuncs.GetClientPackets(1024); // for cleaning packets left from last game.
            ServerSockFuncs.gameRunning = true;
            playerLRS = new DateTime[pCount];
            playerLRS2 = new DateTime[pCount];
            echoDelayMS = new double[pCount];
            echoSendTime = new DateTime[pCount];
            echoWaiting = new bool[pCount];
            for (int i = 0; i < pCount; i++)
            {
                playerLRS[i] = DateTime.MinValue;
                playerLRS2[i] = DateTime.MinValue;
            }

            foreach (LobbyPlayer lp in ServerSockFuncs.lobbyPlayerDict.Values)
            {
                if (!lp.Disconnected)
                {
                    lp.Sock.Send(Encoding.Latin1.GetBytes(lp.Number.ToString() + pCount.ToString() + levelLayout));
                }
            }
            history.Add(CreateInitFrame(pCount));
            gameStartTime = DateTime.Now;
            curFNum = 0;
            UI.Invoke(OnInitGame);
        }
        // Function called when a join lobby request is recieved.
        static public void OnPlayerJoinLobby(string ipStr)
        {
            pCount += 1;
            UI.Invoke(OnLobbyUpdate, new string[] { MakePlayerList() });
            if (pCount == int.Parse(settings["maxPlayers"]))
            {
                InitGame();
            }
        }
        // Function called when a leave lobby request is recieved.
        static public void OnPlayerLeaveLobby(string ipStr)
        {
            pCount -= 1;
            UI.Invoke(OnLobbyUpdate, new string[] { MakePlayerList() });
        }
        // Creates an in game player list for the ui.
        public static string MakePlayerList()
        {
            string pList = "";
            foreach (LobbyPlayer player in ServerSockFuncs.lobbyPlayerDict.Values.OrderBy(p => p.Number))
            {
                pList += "Player " + player.Number + ": " + player.UName + ", " + player.Sock.RemoteEndPoint + "\n";
            }
            return pList;
        }
        // Resets game.
        static public void ResetGame()
        {
            history[history.Count - 1] = CreateInitFrame(pCount);
        }
        // Applies the input at the frame the happened during the specified time. Resimulates until present frame if real input was different from assumed input.
        static void ApplyWithLagComp(Input input, DateTime time, int player)
        {
            for (int i = history.Count() - 1; i > 0; i--)
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
        /* 
            Game loop, handles:
                - recieving player inputs
                - creating an assumed present gamestate using laast inputs from clients
                - applying them with lag compensation (or without, though the lagcomp toggle didn't make it to the final project because its buggy)
                - checking whether game is over
                - sending each player the frame after their last recieved inputs, and all enemy inputs since then.
            returns the game state to draw.
        */
        static public void OnTimerTick()
        {
            DateTime frameStart = DateTime.Now;
            curFNum++;
            NBConsole.WriteLine("Frame start " + frameStart.ToString("mm.ss.fff") + ", Frame num: " + curFNum);

            List<ClientPacket> packets = ServerSockFuncs.GetClientPackets(bufferSize); // recieve player inputs
            if (packets.Count == 0) { NBConsole.WriteLine("no user inputs recieved"); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets"); }

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
                    NBConsole.WriteLine("recv inputs [" + packetInput + "], p" + packetPlayer + " from " + packetTime.ToString("mm.ss.fff") + " during frame that started at " + frameStart.ToString("mm.ss.fff") +
                        "\nMS delay by stamp: " + (frameStart - packetTime).TotalMilliseconds + ", delay by echo: " + echoDelayMS[packetPlayer - 1]);
                    if (packetTime > DateTime.Now)
                    {
                        //throw new Exception("timestamp error");
                    }
                    ApplyWithLagComp(packetInput, packetTime, packetPlayer);
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

            foreach (var ip in ServerSockFuncs.lobbyPlayerDict.Keys) // send state to players
            {
                int thisPlayer = ServerSockFuncs.lobbyPlayerDict[ip].Number;
                if (DateTime.Now - playerLRS[thisPlayer - 1] > TimeSpan.FromSeconds(2) && playerLRS[thisPlayer - 1] != DateTime.MinValue)
                {
                    NBConsole.WriteLine("Disconnected for time reasons");
                    ServerSockFuncs.UpdatePlayerConnectionStatus(ip, true);
                }
                else
                {
                    ServerSockFuncs.UpdatePlayerConnectionStatus(ip, false);
                }
                if (playerLRS[thisPlayer - 1] != DateTime.MinValue && !ServerSockFuncs.lobbyPlayerDict[ip].Disconnected)
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
                    NBConsole.WriteLine("sending: " + sendPacket);
                    ServerSockFuncs.serverSockUdp.SendTo(sendPacket.Serialize(pCount), ip);
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
        // Called on game end. Sends each player a message including the winner, and adds the game to the match history.
        public static void EndGame(int winner)
        {
            OnEndGame();
            gameLength = DateTime.Now - gameStartTime;
            Thread.Sleep(15);
            string playerString = "", winnerString = "Tie";
            foreach (var lp in ServerSockFuncs.lobbyPlayerDict.Values)
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
            foreach (var lp in ServerSockFuncs.lobbyPlayerDict.Values)
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
            DatabaseAccess.AddMatch(new Match(gameStartTime.ToString("dd/MM/yyyy HH:mm"), playerString, winnerString, MinutesToString(gameLength.TotalMinutes)));
            InitLobby();
        }
        // Creates initial frame for the history, according to player count
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
        // turns a minute count into a minutes:seconds count.
        static string MinutesToString(double totalMinutes)
        {
            int mins = (int)totalMinutes;
            int secs = (int)((totalMinutes - mins) * 60);
            return "" + mins + ":" + secs;
        }
    }
}
