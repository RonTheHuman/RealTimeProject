﻿using System;
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
        public static int currentFNum;
        
        public static Form UI;
        public static Action OnInitGame;
        public static Action OnEndGame;
        public static Action<string> OnLobbyUpdate;
        public static int levelLayout;
        public static byte frameMS;

        static Dictionary<string, string> settings;
        static int pCount = 0;
        static DateTime[] playerLRS, playerLRS2;
        static int bufferSize = 1024;
        static bool compensateLag = true;
        static TimeSpan gameLength;
        static DateTime gameStartTime;

        static public void InitServer()
        {
            settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.GetFullPath("serverSettings.txt")));
            Task.Factory.StartNew(() => SocketFuncs.HandleTcpSockets(settings["serverIP"], int.Parse(settings["serverPort"])));
            InitLobby();
        }
        static public void InitLobby()
        {
            pCount = 0;
            history.Clear();
            SocketFuncs.lobbyPlayerDict.Clear();
            SocketFuncs.gameRunning = false;
            SocketFuncs.CreateUdpSocket(settings["serverIP"], int.Parse(settings["serverPort"]));
        }
        static public void InitGame()
        {
            SocketFuncs.gameRunning = true;
            playerLRS = new DateTime[pCount];
            playerLRS2 = new DateTime[pCount];
            for (int i = 0; i < pCount; i++)
            {
                playerLRS[i] = DateTime.MinValue;
                playerLRS2[i] = DateTime.MinValue;
            }
            foreach (LobbyPlayer lp in SocketFuncs.lobbyPlayerDict.Values)
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

        static public void OnPlayerJoinLobby(string ipStr)
        {
            UI.Invoke(OnLobbyUpdate, new string[] { "Player " + (pCount + 1) + ", " + ipStr + " entered\n" });
            pCount += 1;
            if (pCount == int.Parse(settings["maxPlayers"]))
            {
                InitGame();
            }
        }

        static public void OnPlayerExitLobby(string ipStr)
        {
            pCount -= 1;
            UI.Invoke(OnLobbyUpdate, new string[] { "Player " + (pCount + 1) + ", " + ipStr + " left\n" });
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

            List<ClientPacket> packets = SocketFuncs.GetClientPackets(bufferSize);
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

            foreach (var ip in SocketFuncs.lobbyPlayerDict.Keys) // send state to players
            {
                int thisPlayer = SocketFuncs.lobbyPlayerDict[ip].Number;
                if (DateTime.Now - playerLRS[thisPlayer - 1] > TimeSpan.FromSeconds(2))
                {
                    NBConsole.WriteLine("Disconnected for time reasons");
                    SocketFuncs.lobbyPlayerDict[ip].Disconnected = true;
                }
                else
                {
                    SocketFuncs.lobbyPlayerDict[ip].Disconnected = false;
                }
                if (playerLRS[thisPlayer - 1] != DateTime.MinValue && !SocketFuncs.lobbyPlayerDict[ip].Disconnected)
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
                    SocketFuncs.serverSockUdp.SendTo(sendPacket.Serialize(pCount), ip);
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
            foreach (var lp in SocketFuncs.lobbyPlayerDict.Values)
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
            foreach (var lp in SocketFuncs.lobbyPlayerDict.Values)
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
            SocketFuncs.serverSockUdp.Close();
            InitLobby();
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