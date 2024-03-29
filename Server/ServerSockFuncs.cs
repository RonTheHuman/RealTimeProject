﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RealTimeProject
{
    internal class ServerSockFuncs
    {
        public static ConcurrentDictionary<IPEndPoint, LobbyPlayer> lobbyPlayerDict = new ConcurrentDictionary<IPEndPoint, LobbyPlayer>();
        public static Socket serverSockUdp;
        public static bool gameRunning;
        // Runs on a different thread from the ui. Creates Tcp sockets, reads from them and reacts to errors. 
        public static void HandleTcpSockets(string ipstr, int port)
        {
            Socket serverSockTcp = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverSockTcp.Bind(new IPEndPoint(IPAddress.Parse(ipstr), port));
            Console.WriteLine(serverSockTcp.LocalEndPoint);
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
                        UpdatePlayerConnectionStatus((IPEndPoint)sock.RemoteEndPoint, true);
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
                                    UpdatePlayerConnectionStatus((IPEndPoint)sock.RemoteEndPoint, true);
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
                                UpdatePlayerConnectionStatus((IPEndPoint)sock.RemoteEndPoint, true);
                            }
                        }

                    }
                }
            }
        }
        // Gets a tcp message from a client, acts and replies accordingly, using the message type specified by the first byte.
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
                    int pCount = lobbyPlayerDict.Count;
                    lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint] = new LobbyPlayer(uName, pCount + 1, pSock);
                    pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                    ServerFuncs.OnPlayerJoinLobby(pSock.RemoteEndPoint.ToString());
                }
                else
                {
                    if (lobbyPlayerDict.ContainsKey((IPEndPoint)pSock.RemoteEndPoint))
                    {
                        if (lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].UName == uName)
                        {
                            lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Sock = pSock;
                            pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                            int pCount = lobbyPlayerDict.Count;
                            pSock.Send(Encoding.Latin1.GetBytes(lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Number.ToString() + pCount.ToString() + ServerFuncs.levelLayout));
                        }
                    }
                    else if (uName != "guest" && IsUserNameInLobby(uName, out IPEndPoint ipWithName))
                    {
                        lobbyPlayerDict.Remove(ipWithName, out LobbyPlayer removedPlayer);
                        removedPlayer.Sock = pSock;
                        lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint] = removedPlayer;
                        pSock.Send(new byte[1] { (byte)ServerMessageType.Success });
                        int pCount = lobbyPlayerDict.Count;
                        pSock.Send(Encoding.Latin1.GetBytes(lobbyPlayerDict[(IPEndPoint)pSock.RemoteEndPoint].Number.ToString() + pCount.ToString() + ServerFuncs.levelLayout));
                    }
                    else
                        pSock.Send(new byte[1] { (byte)ServerMessageType.Failure });
                }

            }
            else if (msgType == ClientMessageType.LeaveLobby)
            {
                if (!gameRunning)
                {
                    lobbyPlayerDict.Remove((IPEndPoint)pSock.RemoteEndPoint, out LobbyPlayer removedPlayer);
                    foreach (LobbyPlayer player in lobbyPlayerDict.Values)
                    {
                        if (player.Number > removedPlayer.Number)
                        {
                            player.Number -= 1;
                        }
                    }
                    ServerFuncs.OnPlayerLeaveLobby(pSock.RemoteEndPoint.ToString());
                    pSock.Send(new byte[1] { (byte)ServerMessageType.Failure });
                }
            }

            foreach (var ip in lobbyPlayerDict.Keys)
            {
                Console.WriteLine(ip + " " + lobbyPlayerDict[ip].UName + " " + lobbyPlayerDict[ip].Number + " " + lobbyPlayerDict[ip].Sock.RemoteEndPoint);
            }
        }
        // Checks whether recieved udp messages and returns a list of them.
        public static List<ClientPacket> GetClientPackets(int bufferSize)
        {
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
            return packets;
        }
        // Creates a udp socket for the server.
        public static void CreateUdpSocket(string ipstr, int port)
        {
            serverSockUdp = new Socket(SocketType.Dgram, ProtocolType.Udp);
            serverSockUdp.Bind(new IPEndPoint(IPAddress.Parse(ipstr), port));
        }
        // If a player in the game disconnects or reconnects, updates the relevant variable in lobbyPlayerDict and updates other players.
        public static void UpdatePlayerConnectionStatus(IPEndPoint changedPlayerIP, bool disconnected)
        {
            LobbyPlayer changedPlayer = lobbyPlayerDict[changedPlayerIP];
            if (changedPlayer.Disconnected != disconnected)
            {
                changedPlayer.Disconnected = disconnected;
                foreach (LobbyPlayer lp in lobbyPlayerDict.Values)
                {
                    if (lp != changedPlayer)
                    {
                        byte[] toSend = new byte[2];
                        if (disconnected)
                        {
                            toSend[0] = (byte)ServerMessageType.PlayerDisconnected;
                        }
                        else
                        {
                            toSend[0] = (byte)ServerMessageType.PlayerReconnected;
                        }
                        toSend[1] = (byte)changedPlayer.Number;
                        lp.Sock.Send(toSend);
                    }
                }
            }
        }
        // Checks if a user in lobbyPlayerDict has the specified user name. Used for checking reconnection.
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
    }
}
