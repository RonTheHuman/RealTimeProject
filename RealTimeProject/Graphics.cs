using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;
using System.Buffers.Binary;
using System.Collections.Concurrent;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        int right = 0, left = 0, block = 0, attack = 0, thisPlayer;
        int curFNum = 1, recvFNum = 0, frameMS = 17, pCount = 2;
        static int blockCooldown = 0, blockDuration = 40;
        bool grid = false;
        static List<Frame> simHistory = new List<Frame>();
        Socket clientSock = new Socket(SocketType.Dgram, ProtocolType.Udp);
        EndPoint serverEP;
        //Task bulletTimeout = Task.Factory.StartNew(() => Thread.Sleep(1));
         
        byte[] buffer = new byte[1024];
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

        public Graphics()
        {
            InitializeComponent();
        }
        private void Graphics_Load(object sender, EventArgs e)
        {
            //IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress serverAddress = ipHost.AddressList[1];
            int sPort = 12345;
            NBConsole.WriteLine("Enter port for client: ");
            int cPort = int.Parse(Console.ReadLine());
            var sAddress = IPAddress.Parse("10.100.102.20");
            //var sAddress = IPAddress.Parse("192.168.68.112");
            var cAddress = IPAddress.Parse("10.100.102.20");
            //var cAddress = IPAddress.Parse("192.168.68.112");
            //address = IPAddress.Parse("172.16.2.167");
            EndPoint clientEP = new IPEndPoint(cAddress, cPort);
            serverEP = new IPEndPoint(sAddress, sPort);
            EndPoint recieveEP = new IPEndPoint(IPAddress.Any, 0);

            clientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            clientSock.Bind(clientEP);
            clientSock.SendTo(new byte[] { (byte)'a' }, serverEP);
            NBConsole.WriteLine("Waiting server reply");
            clientSock.ReceiveFrom(buffer, ref recieveEP);
            thisPlayer = int.Parse(Encoding.Latin1.GetString(buffer).TrimEnd('\0'));
            NBConsole.WriteLine("You are player " + thisPlayer);
            if (pCount == 1)
                simHistory.Add(new Frame(DateTime.MinValue, new string[] { "0000" }, new GameState(new int[] { 0 }, new int[] { 0 }, new int[] { -blockCooldown }, new char[] { 'r' }, new int[] { 0 })));
            else if (pCount == 2)
                simHistory.Add(new Frame(DateTime.MinValue, new string[] { "0000", "0000" }, new GameState(new int[] { 0, 100 }, new int[] { 0, 0 }, new int[] { -blockCooldown, -blockCooldown }, new char[] { 'r', 'l' }, new int[] { 0, 0 })));
            GameLoopTimer.Interval = frameMS;
            Thread.Sleep(200);
            GameLoopTimer.Enabled = true;
        }


        public static GameState NextState(GameState state, string[] inputs, bool grid)
        {
            int speed = 5;
            if (grid) speed = 50;
            var nextState = new GameState(state);
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i][0] == '1')    //right
                {
                    nextState.positions[i] += speed;
                    nextState.dirs[i] = 'r';
                }
                if (inputs[i][1] == '1')    //left
                {
                    nextState.positions[i] -= speed;
                    nextState.dirs[i] = 'l';
                }
                if (inputs[i][2] == '1')    //block
                {
                    if (state.blockFrames[i] == -blockCooldown)
                    {
                        nextState.blockFrames[i] = blockDuration;
                    }
                }
                if (state.blockFrames[i] > -blockCooldown)
                {
                    nextState.blockFrames[i] -= 1;
                }
                if (inputs[i][3] == '1')    //attack
                {
                    nextState.attacks[i] = 1;
                    if (state.attacks[i] == 0)
                    {
                        if (nextState.dirs[i] == 'r')
                        {
                            for (int j = 0; j < inputs.Length; j++)
                            {
                                if (j != i && state.blockFrames[j] <= 0)
                                {
                                    if (state.positions[i] + 50 < state.positions[j] && state.positions[j] < state.positions[i] + 150)
                                    {
                                        nextState.points[i] += 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < inputs.Length; j++)
                            {
                                if (j != i && state.blockFrames[j] <= 0)
                                {
                                    if (state.positions[i] - 100 < state.positions[j] + 50 && state.positions[j] + 50 < state.positions[i])
                                    {
                                        nextState.points[i] += 1;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    nextState.attacks[i] = 0;
                }
            }
            return nextState;
        }


        private void Draw(GameState state)
        {
            Player1Label.Location = new Point(state.positions[0], Player1Label.Location.Y);
            if (state.blockFrames[0] > 0)
            {
                Player1Label.BackColor = Color.Gray;
            }
            else
            {
                Player1Label.BackColor = Color.MediumTurquoise;
            }
            if (state.dirs[0] == 'r')
            {
                Player1Label.TextAlign = ContentAlignment.MiddleRight;
                if (state.attacks[0] == 1)
                {
                    Attack1Label.Visible = true;
                    Attack1Label.Location = new Point(state.positions[0] + 50, Player1Label.Location.Y + 18);
                }
                else
                {
                    Attack1Label.Visible = false;
                }
            }
            else
            {
                Player1Label.TextAlign = ContentAlignment.MiddleLeft;
                if (state.attacks[0] == 1)
                {
                    Attack1Label.Visible = true;
                    Attack1Label.Location = new Point(state.positions[0] - 100, Player1Label.Location.Y + 18);
                }
                else
                {
                    Attack1Label.Visible = false;
                }
            }
            string scoreText = "Blue score: " + state.points[0];
            if (pCount == 2)
            {
                Player2Label.Location = new Point(state.positions[1], Player2Label.Location.Y);
                scoreText += "\nRed score: " + state.points[1];
                if (state.blockFrames[1] > 0)
                {
                    Player2Label.BackColor = Color.Gray;
                }
                else
                {
                    Player2Label.BackColor = Color.Coral;
                }
                if (state.dirs[1] == 'r')
                {
                    Player2Label.TextAlign = ContentAlignment.MiddleRight;
                    if (state.attacks[1] == 1)
                    {
                        Attack2Label.Visible = true;
                        Attack2Label.Location = new Point(state.positions[1] + 50, Player2Label.Location.Y + 18);
                    }
                    else
                    {
                        Attack2Label.Visible = false;
                    }
                }
                else
                {
                    Player2Label.TextAlign = ContentAlignment.MiddleLeft;
                    if (state.attacks[1] == 1)
                    {
                        Attack2Label.Visible = true;
                        Attack2Label.Location = new Point(state.positions[1] - 100, Player2Label.Location.Y + 18);
                    }
                    else
                    {
                        Attack2Label.Visible = false;
                    }
                }
            }
            ScoreLabel.Text = scoreText;
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            string curInput = "" + right + left + block + attack; // prepare message
            //NBConsole.WriteLine("Preparing message");
            DateTime frameStart = DateTime.Now;
            byte[] inputBytes = Encoding.Latin1.GetBytes(curInput);
            byte[] timeStamp = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(timeStamp, DateTime.Now.Ticks);
            byte[] sendData = new byte[8 + 4];
            inputBytes.CopyTo(sendData, 0);
            timeStamp.CopyTo(sendData, 4);
            //NBConsole.WriteLine(inputBytes.Length + " | " + Convert.ToHexString(inputBytes) + " | " + Convert.ToHexString(timeStamp) + ", " + new DateTime(BinaryPrimitives.ReadInt64BigEndian(timeStamp)).ToString("mm.ss.fff") + " | " + Convert.ToHexString(sendData));
            clientSock.SendToAsync(sendData, SocketFlags.None, serverEP);

            //NBConsole.WriteLine("Creating frame");
            string[] simInputs = new string[pCount]; // create simulated frame
            simHistory.Last().inputs.CopyTo(simInputs, 0);
            simInputs[thisPlayer - 1] = curInput;
            simHistory.Add(new Frame(frameStart, simInputs, NextState(simHistory.Last().state, simInputs, grid)));
            curFNum++;

            NBConsole.WriteLine("Getting packets from server");
            List<string> packets = new List<string>(); // get packets from server
            while (clientSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];
                EndPoint recieveEP = new IPEndPoint(IPAddress.Any, 0);
                clientSock.ReceiveFrom(buffer, ref recieveEP);
                packets.Add(Encoding.Latin1.GetString(buffer).TrimEnd('\0'));
                NBConsole.WriteLine("Recieved " + packets.Last());
            }
            if (packets.Count == 0) { NBConsole.WriteLine("no server data recieved"); }
            //else { NBConsole.WriteLine("got " + packets.Count + " packets"); }
            if (packets.Count() > 0) // deserialize packets and apply to simulated history
            {
                bool foundFrame = false;
                string latestPacket = packets.Last(); // later pick the highest frame because can arrive out of order
                JsonElement[] recvData = JsonSerializer.Deserialize<JsonElement[]>(latestPacket);
                DateTime recvTimeStamp = new DateTime(BinaryPrimitives.ReadInt64BigEndian(recvData[0].Deserialize<byte[]>()));
                string[] recvInputs = recvData[1].Deserialize<string[]>();
                int[] recvPos = recvData[2].Deserialize<int[]>();
                int[] recvPoints = recvData[3].Deserialize<int[]>();
                int[] recvBFrames = recvData[4].Deserialize<int[]>();
                char[] recvDirs = recvData[5].Deserialize<char[]>();
                int[] recvAttacks = recvData[6].Deserialize<int[]>();
                NBConsole.WriteLine("applying data from " + recvTimeStamp.ToString("mm.ss.fff") + " during frame that started at " + frameStart.ToString("mm.ss.fff"));

                for (int i = simHistory.Count() - 1; i >= 0; i--)
                {
                    if (simHistory[i].startTime < recvTimeStamp)
                    {
                        NBConsole.WriteLine("found frame!");
                        foundFrame = true;
                        bool sameFrame = true;
                        sameFrame = sameFrame && Enumerable.SequenceEqual(recvInputs, simHistory[i].inputs);
                        sameFrame = sameFrame && Enumerable.SequenceEqual(recvPos, simHistory[i].state.positions);
                        sameFrame = sameFrame && Enumerable.SequenceEqual(recvPoints, simHistory[i].state.points);
                        sameFrame = sameFrame && Enumerable.SequenceEqual(recvBFrames, simHistory[i].state.blockFrames);
                        sameFrame = sameFrame && Enumerable.SequenceEqual(recvDirs, simHistory[i].state.dirs);
                        sameFrame = sameFrame && Enumerable.SequenceEqual(recvAttacks, simHistory[i].state.attacks);
                        if (!sameFrame)
                        {
                            NBConsole.WriteLine("Rollbacking to frame " + simHistory[i].startTime.ToString("mm.ss.fff"));
                            simHistory[i].state = new GameState(recvPos, recvPoints, recvBFrames, recvDirs, recvAttacks);
                            for(int j = i + 1; j < simHistory.Count; j++)
                            {
                                string[] correctInputs = new string[pCount];
                                for (int k = 0; k < pCount; k++)
                                {
                                    if (k != thisPlayer - 1)
                                    {
                                        correctInputs[k] = recvInputs[k];
                                    }
                                }
                                correctInputs[thisPlayer - 1] = simHistory[j].inputs[thisPlayer - 1];
                                simHistory[j].inputs = correctInputs;
                                simHistory[j].state = NextState(simHistory[j - 1].state, correctInputs, grid);
                            }
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
            NBConsole.WriteLine("updated state: " + simHistory.Last().state.ToString());
            Draw(simHistory.Last().state);

            //old history deletion, maybe needed?
            if (simHistory.Count >= 200)
            {
                simHistory.RemoveRange(0, 100);
            }
        }

        private void Graphics_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    right = 1;
                    break;
                case Keys.Left:
                    left = 1;
                    break;
                case Keys.Z:
                    block = 1;
                    break;
                case Keys.X:
                    attack = 1;
                    break;
            }
        }

        private void Graphics_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    right = 0;
                    break;
                case Keys.Left:
                    left = 0;
                    break;
                case Keys.Z:
                    block = 0;
                    break;
                case Keys.X:
                    attack = 0;
                    break;
            }
        }

        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            TimeLabel.Text = "Time: " + DateTime.Now.ToString("mm.ss.fff") + ", Frame: " + curFNum;
        }
    }
}