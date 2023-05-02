using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RealTimeProject
{
    internal class SocketFuncs
    {
        public static Socket clientSock, clientSockTcp;
        static IPEndPoint clientEP, serverEP;
        public static void InitSockets(int serverPort, int clientPort, string serverIP, string clientIP)
        {
            var sAddress = IPAddress.Parse(serverIP);
            var cAddress = IPAddress.Parse(clientIP);
            var actualClientPort = FindAvailablePort(clientPort);
            Console.WriteLine("Using port " + actualClientPort);
            clientEP = new IPEndPoint(cAddress, actualClientPort);
            serverEP = new IPEndPoint(sAddress, serverPort);

            clientSockTcp.Bind(clientEP);
            clientSockTcp.Connect(serverEP);
            clientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            clientSock.Bind(clientEP);
        }

        static int FindAvailablePort(int startPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startPort
                               select n.LocalEndPoint.Port);

            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startPort
                               select n.Port);

            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startPort
                               select n.Port);

            portArray.Sort();

            for (int i = startPort; i < ushort.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;
            return 0;
        }

        public static bool SignIn(string uName, string password)
        {
            List<byte> toSend = new List<byte> { (byte)ClientMessageType.CheckSignIn };
            toSend.AddRange(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(new string[] { uName, password })));
            clientSockTcp.Send(toSend.ToArray());
            byte[] buffer = new byte[8];
            clientSockTcp.Receive(buffer);
            return buffer[0] == (byte)ServerMessageType.Success;
        }

        public static bool SignUp(string uName, string password)
        {
            List<byte> toSend = new List<byte> { (byte)ClientMessageType.SignUp };
            toSend.AddRange(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(new string[] { uName, password })));
            clientSockTcp.Send(toSend.ToArray());
            byte[] buffer = new byte[8];
            clientSockTcp.Receive(buffer);
            return buffer[0] == (byte)ServerMessageType.Success;
        }

        public static bool JoinLobbyRequest(string uName, ref string recvData)
        {
            byte[] buffer = new byte[8];
            List<byte> toSend = new List<byte> { (byte)ClientMessageType.JoinLobby };
            toSend.AddRange(Encoding.Latin1.GetBytes(uName));
            clientSockTcp.Send(toSend.ToArray());
            NBConsole.WriteLine("Waiting server reply");
            int recievedBytes = clientSockTcp.Receive(buffer);
            if (buffer[0] == (byte)ServerMessageType.Success)
            {
                if (recievedBytes > 1)
                {
                    recvData = Encoding.Latin1.GetString(buffer[1..]);
                }
                return true;
            }
            else
                return false;
        }

        public static void SendUdp(byte[] sendData)
        {
            clientSock.SendTo(sendData, SocketFlags.None, serverEP);
        }

        public static string GetGameData()
        {
            byte[] buffer = new byte[8];
            clientSockTcp.Receive(buffer);
            return Encoding.Latin1.GetString(buffer);
        }

        public static List<Match> GetMatchesWithUser(string uName)
        {
            List<byte> toSend = new List<byte> { (byte)ClientMessageType.GetMatchesWithUser };
            toSend.AddRange(Encoding.Latin1.GetBytes(uName));
            clientSockTcp.Send(toSend.ToArray());
            byte[] tcpBuffer = new byte[1024];
            int bytesRecieved = clientSockTcp.Receive(tcpBuffer);
            string dataString = Encoding.Latin1.GetString(tcpBuffer[..bytesRecieved]);
            while (dataString[dataString.Length - 1] != '|')
            {
                bytesRecieved = clientSockTcp.Receive(tcpBuffer);
                dataString += Encoding.Latin1.GetString(tcpBuffer[..bytesRecieved]);
            }
            return JsonSerializer.Deserialize<List<Match>>(dataString[..^1]);
        }
    }
}
