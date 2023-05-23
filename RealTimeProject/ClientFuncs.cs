using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RealTimeProject
{
    internal class ClientFuncs
    {
        public static string uName = "guest";
        public static Action OnJoinLobby, OnJoinFail;
        public static Action<string> OnEndGame;
        public static System.Windows.Forms.Timer timer;
        public static ClientUI UI;
        public static bool inLobby;
        public static int thisPlayer, pCount, levelLayout;
        public static bool[] pDisconnectedArr;

        static Dictionary<string, string> settings;
        static string[] playerColor = new string[] { "Blue", "Red", "Yellow", "purple" };

        static List<Frame> simHistory = new List<Frame>(200);
        static int curFNum;
        static List<Input> unackedInputs = new List<Input>(200);

        static byte[] gameTcpBuffer = new byte[1024];
        public static void InitClient()
        {
            settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.GetFullPath("clientSettings.txt")));
            ClientSockFuncs.InitSockets(int.Parse(settings["serverPort"]), int.Parse(settings["clientPort"]), settings["serverIP"], settings["clientIP"]);
        }
        // the function called by ui to join a game lobby
        public static void JoinLobby()
        {
            inLobby = true;
            Task.Factory.StartNew(() => JoinLobbyAsync());
        }
        // Function running on another thread, waiting for a reply from the server about the start of the game. When game starts initializes vairables. 
        static void JoinLobbyAsync()
        {
            string recvData = "";
            if (ClientSockFuncs.JoinLobbyRequest(uName, ref recvData))
            {
                bool stayedInLobby = true;
                if (recvData.Length > 0)
                {
                    Console.WriteLine("All in one packet");
                }
                else
                {
                    stayedInLobby = ClientSockFuncs.GetGameData(ref recvData);
                }
                if (stayedInLobby)
                {
                    thisPlayer = int.Parse(recvData[0] + "");
                    pCount = int.Parse(recvData[1] + "");
                    levelLayout = int.Parse(recvData[2] + "");
                    NBConsole.WriteLine("You are player " + thisPlayer);
                    ClientSockFuncs.GetServerPackets(1024); // for cleaning packets left from last game.
                    InitSimulatedHistory();
                    pDisconnectedArr = new bool[pCount];
                    curFNum = 0;
                    Thread.Sleep(200);
                    UI.Invoke(OnJoinLobby);
                }
                else
                {
                    NBConsole.WriteLine("Left Lobby");
                }
            }
            else
            {
                UI.Invoke(OnJoinFail);
                NBConsole.WriteLine("Game is already running.");
            }
        }
        /* 
            Game loop, handles:
                - recieving tcp updates about game end, and disconnection or reconnection of other players
                - recieving udp game state data
                - creating a simulated history using combined client sim and enemy extrapolation
                - if needed, creating seperatly simulated gamestates
            returns the game state to draw.
        */
        public static GameState OnTimerTick(Input curInput, bool fullSim, bool clientSim, bool enemySim)
        {
            DateTime frameStart = DateTime.Now;
            NBConsole.WriteLine("Current input: [" + curInput + "]" + " current frame num: " + curFNum + " frameStart: " + frameStart.ToString("mm.ss.fff"));

            byte[] timeStamp = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(timeStamp, frameStart.Ticks);
            byte[] sendData = new byte[8 + 1];
            sendData[0] = (byte)curInput;
            timeStamp.CopyTo(sendData, 1);
            ClientSockFuncs.SendUdp(sendData);

            if (ClientSockFuncs.clientSockTcp.Poll(1, SelectMode.SelectRead)) // check for end game message or for update about other player connection
            {
                int byteCount =  ClientSockFuncs.clientSockTcp.Receive(gameTcpBuffer);
                if (gameTcpBuffer[0] == (byte)ServerMessageType.GameEnd)
                {
                    string winner = Encoding.Latin1.GetString(gameTcpBuffer[1..byteCount]);
                    if (winner[..^1] == "guest")
                    {
                        switch (winner[^1])
                        {
                            case '1':
                                winner += " (" + playerColor[0] + ")";
                                break;
                            case '2':
                                winner += " (" + playerColor[1] + ")";
                                break;
                            case '3':
                                winner += " (" + playerColor[2] + ")";
                                break;
                            case '4':
                                winner += " (" + playerColor[3] + ")";
                                break;
                        }

                    }
                    inLobby = false;
                    OnEndGame(winner);
                }
                else if (gameTcpBuffer[0] == (byte)ServerMessageType.PlayerDisconnected)
                {
                    int player = gameTcpBuffer[1];
                    pDisconnectedArr[player - 1] = true;
                }
                else if (gameTcpBuffer[0] == (byte)ServerMessageType.PlayerReconnected)
                {
                    int player = gameTcpBuffer[1];
                    pDisconnectedArr[player - 1] = false;
                }
                else
                {
                    throw new Exception("wrong tcp message while in game");
                }
            }

            List<byte[]> packets = ClientSockFuncs.GetServerPackets(1024); // gets server packets
            if (packets.Count == 0) { NBConsole.WriteLine("no server data recieved"); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets"); }

            Input[] simInputs = new Input[pCount]; // create simulated frame
            simHistory.Last().Inputs.CopyTo(simInputs, 0);
            simInputs[thisPlayer - 1] = curInput;
            simHistory.Add(new Frame(frameStart, simInputs, GameLogic.NextState(simHistory.Last().Inputs, simInputs, simHistory.Last().State, levelLayout)));
            curFNum++;
            ServerGamePacket lastServerPacket = null;
            if (packets.Count() > 0) // deserialize packets and apply to simulated history
            {
                ServerGamePacket servPacket = ServerGamePacket.Deserialize(packets.Last(), pCount); // pick the latest frame because can arrive out of order
                for (int i = 0; i < packets.Count - 1; i++)
                {
                    ServerGamePacket temp = ServerGamePacket.Deserialize(packets[i], pCount);
                    if (temp.TimeStamp > servPacket.TimeStamp)
                    {
                        servPacket = temp;
                    }
                }
                lastServerPacket = servPacket;
                timer.Interval = lastServerPacket.FrameMS;
                unackedInputs.Clear();
                NBConsole.WriteLine("applying data from " + servPacket.TimeStamp.ToString("mm.ss.fff") + ". data:\n" + servPacket.ToString());
                //client simulation and lagcomp on enemies
                bool foundFrame = false;
                for (int i = simHistory.Count() - 1; i > 0; i--)
                {
                    if (simHistory[i - 1].StartTime <= servPacket.Frame.StartTime)
                    {
                        foundFrame = true;
                        NBConsole.WriteLine("resimulation start: " + i + " history end: " + (simHistory.Count - 1) + ", count: " + (simHistory.Count - 1 - i));
                        if (pCount > 1)
                            NBConsole.WriteLine(" server sent " + servPacket.EnemyInputs[0].Length + " eInputs");
                        simHistory[i] = servPacket.Frame;
                        for (int j = i + 1; j < simHistory.Count(); j++)
                        {
                            Input[] correctInputs = new Input[pCount];
                            for (int k = 0; k < pCount; k++)
                            {
                                if (k == thisPlayer - 1)
                                {
                                    correctInputs[k] = simHistory[j].Inputs[k];
                                    unackedInputs.Add(correctInputs[k]);
                                }
                                else
                                {
                                    int offset = 0;
                                    if (k > thisPlayer - 1)
                                        offset = -1;
                                    if (j - i - 1 < servPacket.EnemyInputs[k + offset].Length)
                                    {
                                        correctInputs[k] = servPacket.EnemyInputs[k + offset][j - i - 1];
                                    }
                                    else
                                    {
                                        correctInputs[k] = simHistory[j - 1].Inputs[k];
                                    }
                                }
                            }
                            simHistory[j].State = GameLogic.NextState(simHistory[j - 1].Inputs, correctInputs, simHistory[j - 1].State, levelLayout);
                            simHistory[j].Inputs = correctInputs;
                            //NBConsole.WriteLine("P" + thisPlayer + ": pInput= (" + simHistory[j - 1].Inputs[thisPlayer - 1] + "), cInput= (" + correctInputs[thisPlayer - 1] + "), " +
                            //    "start state = " + simHistory[j - 1].State + ", after: Pos= " + simHistory[j].State.PStates[thisPlayer - 1].Pos + ", Vel= " + simHistory[j].State.PStates[thisPlayer - 1].Vel);
                        }
                        break;
                    }
                }
                if (!foundFrame)
                {
                    NBConsole.WriteLine("didn't find :(");
                    throw new Exception();
                }
            }
            else
            {
                unackedInputs.Add(curInput);
            }

            if (fullSim)
            {
                // show the final frame from the simulated history (with causality)
                NBConsole.WriteLine("updated state: " + simHistory.Last().State.ToString());
                NBConsole.WriteLine("Took " + (DateTime.Now - frameStart).Milliseconds);
                return simHistory.Last().State;
            }
            else
            {
                // show the frame after the last player action recieved from server, no extra simulation
                if (!clientSim && !enemySim)
                {
                    if (lastServerPacket != null)
                    {
                        if (packets.Count() > 0) // deserialize packets and apply to simulated history
                        {
                            NBConsole.WriteLine("applying data from " + lastServerPacket.TimeStamp.ToString("mm.ss.fff") +
                                " during frame that started at " + frameStart.ToString("mm.ss.fff"));
                            return lastServerPacket.Frame.State;
                        }
                    }
                }
                else
                {
                    if (lastServerPacket != null)
                    {
                        GameState stateToDraw = new GameState(lastServerPacket.Frame.State);
                        // simulate the catch-up frames for the enemies sent by the server
                        for (int i = 0; i < pCount; i++)
                        {
                            if (i != thisPlayer - 1)
                            {
                                int offset = 0;
                                if (i > thisPlayer - 1)
                                {
                                    offset = -1;
                                }
                                Input[] enemyInputs = new Input[lastServerPacket.EnemyInputs[i + offset].Length + 1];
                                enemyInputs[0] = lastServerPacket.Frame.Inputs[i];
                                lastServerPacket.EnemyInputs[i + offset].CopyTo(enemyInputs, 1);
                                stateToDraw.PStates[i] = GameLogic.SimulatePlayerState(stateToDraw.PStates[i], enemyInputs, levelLayout);
                            }
                        }
                        //simulate client inputs that didn't reach the server
                        if (clientSim)
                        {
                            stateToDraw.PStates[thisPlayer - 1] = GameLogic.SimulatePlayerState(stateToDraw.PStates[thisPlayer - 1], unackedInputs.ToArray(), levelLayout);
                        }
                        if (enemySim && lastServerPacket.EnemyInputs.Length != 0)
                        {
                            if (lastServerPacket.EnemyInputs[0].Length != 0 && unackedInputs.Count > 0)
                            {
                                for (int i = 0; i < pCount; i++)
                                {
                                    if (i != thisPlayer - 1)
                                    {
                                        int offset = 0;
                                        if (i > thisPlayer - 1)
                                        {
                                            offset = -1;
                                        }
                                        Input[] enemyInputs = new Input[Math.Abs(unackedInputs.Count - lastServerPacket.EnemyInputs[0].Length) + 1];
                                        for (int j = 0; j < enemyInputs.Length; j++)
                                        {
                                            enemyInputs[j] = lastServerPacket.EnemyInputs[i + offset][^1];
                                        }
                                        stateToDraw.PStates[i] = GameLogic.SimulatePlayerState(stateToDraw.PStates[i], enemyInputs, levelLayout);
                                    }
                                }
                            }

                        }
                        return stateToDraw;
                    }
                }
            }
            return null;
        }
        // Adds first frame to simHistory, according to player count
        private static void InitSimulatedHistory()
        {
            simHistory.Clear();
            if (pCount == 1)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None }, GameLogic.InitialState(1)));
            else if (pCount == 2)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None }, GameLogic.InitialState(2)));
            else if (pCount == 3)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None, Input.None }, GameLogic.InitialState(3)));
            else if (pCount == 4)
                simHistory.Add(new Frame(DateTime.MinValue, new Input[] { Input.None, Input.None, Input.None, Input.None }, GameLogic.InitialState(4)));
        }
    }
}
