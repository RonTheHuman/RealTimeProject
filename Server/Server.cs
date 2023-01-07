using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace RealTimeProject
{
    internal class Server
    {
        const int bufferSize = 1024, pCount = 1, frameMS = 17;
        static bool grid = false;
        static bool compensateLag = true;
        //const int simLag = 0;

        static List<Frame> history = new List<Frame> ();
        static int curFNum = 0, hSFNum = 0; //current frame number, history start frame number
        static int[] playerLRFNum = new int[pCount]; //last recieved frame num for each player
        static Socket serverSock = new Socket(SocketType.Dgram, ProtocolType.Udp);
        static Dictionary<IPEndPoint, int> playerIPs = new Dictionary<IPEndPoint, int>();



        //static TimeSpan[] pRTTs = new TimeSpan[] {TimeSpan.Zero, TimeSpan.Zero};
        //static Action<Socket, TimeSpan[], int> getRTT = (Socket sock, TimeSpan[] pRTTs, int i) =>
        //{
        //    sock.Send(new byte[1]);
        //    Console.WriteLine("[sent echo]");
        //    DateTime sendTime = DateTime.Now;
        //    sock.Receive(new byte[1]);
        //    Console.WriteLine("[recvd echo]");
        //    pRTTs[i] = DateTime.Now - sendTime;
        //    Console.WriteLine("[p" + (i+1) + " rtt: " + pRTTs[0].TotalMilliseconds +  ", comp: " + compensateLag + "]");
        //};
        //commands start with the player number. ex: "1Shoot"


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


        static void Rollback(string input, int fNum, int player)
        {
            if (history[fNum].inputs[player - 1] != input)
            {
                for (int i = fNum - hSFNum; i < history.Count; i++)
                {
                    Frame temp = history[i];
                    temp.inputs[player - 1] = input;
                    temp.state = CommonCode.NextState(history[i - 1].state, history[i].inputs, grid);
                    history[i] = temp;
                }
            }
        }


        static TimeSpan GameLoop(DateTime st)
        {
            curFNum++;
            //NBConsole.WriteLine("Started at " + st.Millisecond);
            List<string> packets = new List<string>();
            while (serverSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                serverSock.ReceiveFrom(buffer, ref clientEP);
                packets.Add(playerIPs[(IPEndPoint)clientEP].ToString() + Encoding.Latin1.GetString(buffer));
                NBConsole.WriteLine("Recieved " + packets.Last() + ", current frame " + curFNum + "| " + st.Millisecond);
            }
            if (packets.Count == 0) { NBConsole.WriteLine("no user inputs recieved" + "| " + st.Millisecond); }
            else { NBConsole.WriteLine("got " + packets.Count + " packets" + "| " + st.Millisecond); }

            //now each packet has (in this order): pnum, right, left, block, attack, fNum
            string[] latestInputs = new string[pCount];
            int packetPlayer, packetFrame;
            string packetInput;
            for (int i = 0; i < packets.Count; i++)
            {
                packetPlayer = int.Parse("" + packets[i][0]);
                packetInput = packets[i][1..5];
                packetFrame = int.Parse(packets[i][5..]);
                if (packetFrame == curFNum)
                {
                    latestInputs[packetPlayer - 1] = packetInput;
                    packets.RemoveAt(i);
                    i--;
                    playerLRFNum[packetPlayer - 1] = curFNum;
                }
            }
            for (int i = 0; i < pCount; i++)
            {
                if (latestInputs[i] == null)
                {
                    latestInputs[i] = history.Last().inputs[i];
                }
            }
            history.Add(new Frame(latestInputs, CommonCode.NextState(history.Last().state, latestInputs, grid)));

            for (int i = 0; i < packets.Count; i++)
            {
                packetPlayer = int.Parse("" + packets[i][0]);
                packetInput = packets[i][1..5];
                packetFrame = int.Parse(packets[i][5..]);
                if (packetFrame < curFNum || true)
                {
                    NBConsole.WriteLine("Rollbacking" + "| " + st.Millisecond);
                    Rollback(packetInput, packetFrame, packetPlayer);
                }
                if (packetFrame > playerLRFNum[packetPlayer - 1])
                {
                    playerLRFNum[packetPlayer - 1] = packetFrame;
                }
            }

            foreach (var ip in playerIPs.Keys)
            {
                //serverSock.SendToAsync(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(history.Last())), SocketFlags.None, ip);
                object[] sendData = new object[6];
                sendData[0] = playerLRFNum[playerIPs[ip] - 1];
                sendData[1] = history.Last().inputs;
                sendData[2] = history.Last().state.positions;
                sendData[3] = history.Last().state.points;
                sendData[4] = history.Last().state.blockFrames;
                sendData[5] = history.Last().state.dirs;
                Console.WriteLine(JsonSerializer.Serialize(sendData));
                serverSock.SendTo(Encoding.Latin1.GetBytes(JsonSerializer.Serialize(sendData)), ip);
                //Console.WriteLine("last recieved fNum from player " + playerIPs[ip] + ", " + playerLRFNum[playerIPs[ip] - 1]);
            }

            //if (history.Count >= 200)
            //{
            //    history.RemoveRange(0, 100);
            //    hSFNum += 100;
            //}
            
            string printMsg = "History:\n";
            foreach (var f in history.Skip(history.Count - 20))
            {
                printMsg += f.ToString() + "\n";
            }
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
            //Console.WriteLine(address);
            int cPort = 12345, sPort = 12346;
            serverSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            byte[] buffer = new byte[bufferSize];
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);

            serverSock.Bind(new IPEndPoint(sAddress, sPort));
            for (int i = 0; i < pCount; i++)
            {
                Console.WriteLine("Waiting for player " + (i + 1));
                serverSock.ReceiveFrom(buffer, ref clientEP);
                string clientIPStr = clientEP.ToString();
                //clientIPStr = clientIPStr.Remove(clientIPStr.IndexOf(':'));
                Console.WriteLine(clientIPStr + " entered");
                //var test = clientEP.Serialize();
                //Console.WriteLine(test);
                playerIPs[new IPEndPoint(IPAddress.Parse(clientIPStr), cPort)] = i + 1;
            }
            foreach (var ip in playerIPs.Keys)
            {
                serverSock.SendTo(Encoding.Latin1.GetBytes(playerIPs[ip].ToString()), ip);
            }

            if (pCount == 1)
                history.Add(new Frame(new string[] { "0000" }, new GameState(new int[] { 0 }, new int[] { 0 }, new int[] { -300 }, new char[] { 'r' })));
            else if (pCount == 2)
                history.Add(new Frame(new string[] { "0000", "0000" }, new GameState(new int[] { 0, 100 }, new int[] { 0, 0 }, new int[] { -300, -300 }, new char[] { 'r', 'l' })));

            //serverSock.Poll(-1, SelectMode.SelectRead);
            while (true)
            {
                TimeSpan duration = GameLoop(DateTime.Now);
                NBConsole.WriteLine("took " + duration.TotalMilliseconds + " ms");
                if (frameMS - duration.TotalMilliseconds > 0)
                {
                    Thread.Sleep(frameMS - (int)duration.TotalMilliseconds);
                }
            }
        }
    }
}
