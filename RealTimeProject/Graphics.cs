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
        int curFNum = 1, recvFNum = 0, frameMS = 15, pCount = 2;
        static int blockCooldown = 0, blockDuration = 40;
        bool grid = false, simulate = true;
        static List<Frame> simHistory = new List<Frame>();
        Socket clientSock = new Socket(SocketType.Dgram, ProtocolType.Udp);
        EndPoint serverEP;
         
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
            IPAddress autoAdress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1];
            string[] adresses = new string[3] { "172.16.2.167", "10.100.102.20", "192.168.68.112" };
            int sPort = 12345;
            NBConsole.WriteLine("Enter port for client: ");
            int cPort = int.Parse(Console.ReadLine());
            var sAddress = IPAddress.Parse(adresses[1]);
            var cAddress = IPAddress.Parse(adresses[1]);
            EndPoint clientEP = new IPEndPoint(cAddress, cPort);
            serverEP = new IPEndPoint(sAddress, sPort);

            clientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            clientSock.Bind(clientEP);
            clientSock.SendTo(new byte[] { (byte)'a' }, serverEP);
            NBConsole.WriteLine("Waiting server reply");
            EndPoint recieveEP = new IPEndPoint(IPAddress.Any, 0);
            clientSock.ReceiveFrom(buffer, ref recieveEP);
            thisPlayer = int.Parse(Encoding.Latin1.GetString(buffer).TrimEnd('\0'));
            NBConsole.WriteLine("You are player " + thisPlayer);
            if (pCount == 1)
                simHistory.Add(new Frame(DateTime.MinValue, new string[] { "0000" }, GameState.InitialState(1)));
            else if (pCount == 2)
                simHistory.Add(new Frame(DateTime.MinValue, new string[] { "0000", "0000" }, GameState.InitialState(2)));
            GameLoopTimer.Interval = frameMS;
            Thread.Sleep(200);
            GameLoopTimer.Enabled = true;

            if (simulate)
                Text = "Simulating";
            else
                Text = "Not Simulating";
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
                    AttackLabel1.Visible = true;
                    AttackLabel1.Location = new Point(state.positions[0] + 50, Player1Label.Location.Y + 18);
                }
                else
                {
                    AttackLabel1.Visible = false;
                }
            }
            else
            {
                Player1Label.TextAlign = ContentAlignment.MiddleLeft;
                if (state.attacks[0] == 1)
                {
                    AttackLabel1.Visible = true;
                    AttackLabel1.Location = new Point(state.positions[0] - 100, Player1Label.Location.Y + 18);
                }
                else
                {
                    AttackLabel1.Visible = false;
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
                        AttackLabel2.Visible = true;
                        AttackLabel2.Location = new Point(state.positions[1] + 50, Player2Label.Location.Y + 18);
                    }
                    else
                    {
                        AttackLabel2.Visible = false;
                    }
                }
                else
                {
                    Player2Label.TextAlign = ContentAlignment.MiddleLeft;
                    if (state.attacks[1] == 1)
                    {
                        AttackLabel2.Visible = true;
                        AttackLabel2.Location = new Point(state.positions[1] - 100, Player2Label.Location.Y + 18);
                    }
                    else
                    {
                        AttackLabel2.Visible = false;
                    }
                }
            }
            ScoreLabel.Text = scoreText;
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            string curInput = "" + right + left + block + attack; // prepare message
            NBConsole.WriteLine("Current input: [" + curInput + "]" + " current frame num: " + curFNum);
            DateTime frameStart = DateTime.Now;
            byte[] inputBytes = Encoding.Latin1.GetBytes(curInput);
            byte[] timeStamp = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(timeStamp, frameStart.Ticks);
            byte[] sendData = new byte[8 + 4];
            inputBytes.CopyTo(sendData, 0);
            timeStamp.CopyTo(sendData, 4);
            clientSock.SendToAsync(sendData, SocketFlags.None, serverEP);
            //NBConsole.WriteLine(inputBytes.Length + " | " + Convert.ToHexString(inputBytes) + " | " + Convert.ToHexString(timeStamp) + ", " + new DateTime(BinaryPrimitives.ReadInt64BigEndian(timeStamp)).ToString("mm.ss.fff") + " | " + Convert.ToHexString(sendData));

            //NBConsole.WriteLine("Getting packets from server");
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
            else { NBConsole.WriteLine("got " + packets.Count + " packets"); }


            if (simulate)
            {
                string[] simInputs = new string[pCount]; // create simulated frame
                simHistory.Last().Inputs.CopyTo(simInputs, 0);
                simInputs[thisPlayer - 1] = curInput;
                simHistory.Add(new Frame(frameStart, simInputs, GameState.NextState(simHistory.Last().State, simInputs, grid)));
                curFNum++;

                if (packets.Count() > 0) // deserialize packets and apply to simulated history
                {
                    bool foundFrame = false;
                    string latestPacket = packets.Last(); // later pick the highest frame because can arrive out of order
                    ServerPacket servPacket = ServerPacket.Deserialize(latestPacket, pCount);
                    NBConsole.WriteLine("applying data from " + servPacket.TimeStamp.ToString("mm.ss.fff") + " during frame that started at " + frameStart.ToString("mm.ss.fff"));
                    //client simulation and lagcomp on enemies
                    for (int i = simHistory.Count() - 1; i > 0; i--)
                    {
                        if (simHistory[i - 1].StartTime <= servPacket.Frame.StartTime)
                        {
                            foundFrame = true;
                            NBConsole.WriteLine("i: " + i + " history end: " + (simHistory.Count - 1) + ", " + (simHistory.Count - 1 - i));
                            if (pCount > 1)
                                NBConsole.WriteLine(" server has " + servPacket.EnemyInputs[0].Length);
                            simHistory[i] = servPacket.Frame;
                            for (int j = i + 1; j < simHistory.Count(); j++)
                            {
                                string[] correctInputs = new string[pCount];
                                for (int k = 0; k < pCount; k++)
                                {
                                    if (k == thisPlayer - 1)
                                    {
                                        correctInputs[k] = simHistory[j].Inputs[k];
                                    }
                                    else
                                    {
                                        int offset = 0;
                                        if (k > thisPlayer - 1)
                                            offset = -1;
                                        if (servPacket.EnemyInputs[k + offset].Length > j - i - 1)
                                        {
                                            correctInputs[k] = servPacket.EnemyInputs[k + offset][j - i - 1];
                                            NBConsole.WriteLine(correctInputs[k]);
                                        }
                                        else
                                        {
                                            correctInputs[k] = simHistory[j - 1].Inputs[k];
                                            NBConsole.WriteLine(correctInputs[k]);
                                        }
                                    }
                                }
                                simHistory[j].State = GameState.NextState(simHistory[j - 1].State, correctInputs, false);
                                simHistory[j].Inputs = correctInputs;
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
                NBConsole.WriteLine("updated state: " + simHistory.Last().State.ToString() + "\n");
                Draw(simHistory.Last().State);
            }
            else
            {
                if (packets.Count() > 0) // deserialize packets and apply to simulated history
                {
                    string latestPacket = packets.Last(); // later pick the highest frame because can arrive out of order
                    ServerPacket servPacket = ServerPacket.Deserialize(latestPacket, pCount);
                    NBConsole.WriteLine("applying data from " + servPacket.TimeStamp.ToString("mm.ss.fff") + 
                        " during frame that started at " + frameStart.ToString("mm.ss.fff"));
                    Draw(servPacket.Frame.State);
                }
            }

            //old history deletion, maybe needed?
            //if (simHistory.Count >= 200)
            //{
            //    simHistory.RemoveRange(0, 100);
            //}
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
                case Keys.S:
                    simulate = !simulate;
                    if (simulate)
                        Text = "Simulating";
                    else
                        Text = "Not Simulating";
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