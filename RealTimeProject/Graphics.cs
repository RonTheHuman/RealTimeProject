using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;
using System.Buffers.Binary;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        int right = 0, left = 0, block = 0, attack = 0, thisPlayer, curFNum = 1, recvFNum = 0, frameMS = 17, pCount = 2;
        bool grid = false;
        static List<Frame> simHistory = new List<Frame>();
        Socket clientSock = new Socket(SocketType.Dgram, ProtocolType.Udp);
        EndPoint serverEP;
        //Task bulletTimeout = Task.Factory.StartNew(() => Thread.Sleep(1));
         
        byte[] buffer = new byte[1024];
        public Graphics()
        {
            InitializeComponent();
        }
        private void Graphics_Load(object sender, EventArgs e)
        {
            //IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress serverAddress = ipHost.AddressList[1];
            int sPort = 12345;
            Console.WriteLine("Enter port for client: ");
            int cPort = int.Parse(Console.ReadLine());
            var sAddress = IPAddress.Parse("10.100.102.20");
            var cAddress = IPAddress.Parse("10.100.102.20");
            //address = IPAddress.Parse("172.16.2.167");
            EndPoint clientEP = new IPEndPoint(cAddress, cPort);
            serverEP = new IPEndPoint(sAddress, sPort);
            EndPoint recieveEP = new IPEndPoint(IPAddress.Any, 0);

            clientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            clientSock.Bind(clientEP);
            clientSock.SendTo(new byte[] { (byte)'a' }, serverEP);
            Console.WriteLine("Waiting server reply");
            clientSock.ReceiveFrom(buffer, ref recieveEP);
            thisPlayer = int.Parse(Encoding.Latin1.GetString(buffer).TrimEnd('\0'));
            Console.WriteLine("You are player " + thisPlayer);
            if (pCount == 1)
                simHistory.Add(new Frame(new string[] { "0000" }, new GameState(new int[] { 0 }, new int[] { 0 }, new int[] { -300 }, new char[] { 'r' }, new int[] { 0 })));
            else if (pCount == 2)
                simHistory.Add(new Frame(new string[] { "0000", "0000" }, new GameState(new int[] { 0, 100 }, new int[] { 0, 0 }, new int[] { -300, -300 }, new char[] { 'r', 'l' }, new int[] { 0, 0 })));
            GameLoopTimer.Interval = frameMS;
            Thread.Sleep(1000);
            GameLoopTimer.Enabled = true;
        }


        public static GameState NextState(GameState state, string[] inputs, bool grid)
        {
            int speed = 5, blockDur = 20, blockCooldown = 300;
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
                        nextState.blockFrames[i] = blockDur;
                    }
                }
                if (state.blockFrames[i] > -blockCooldown)
                {
                    nextState.blockFrames[i] -= 1;
                }
                if (inputs[i][3] == '1')    //attack
                {
                    nextState.attacks[i] = 1;
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
            string curInput = "" + right + left + block + attack;
            byte[] inputBytes = Encoding.Latin1.GetBytes(curInput);
            byte[] timeStamp = new byte[8];
            BinaryPrimitives.WriteInt64BigEndian(timeStamp, DateTime.Now.Ticks);
            byte[] sendData = new byte[8 + inputBytes.Length];
            inputBytes.CopyTo(sendData, 0);
            timeStamp.CopyTo(sendData, inputBytes.Length);
            clientSock.SendToAsync(sendData, SocketFlags.None, serverEP);
            string[] simInputs = new string[pCount];
            simHistory.Last().inputs.CopyTo(simInputs, 0);
            simInputs[thisPlayer - 1] = curInput;
            simHistory.Add(new Frame(simInputs, NextState(simHistory.Last().state, simInputs, grid)));
            curFNum++;
            List<string> packets = new List<string>();
            while (clientSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                clientSock.ReceiveFrom(buffer, ref clientEP);
                packets.Add(Encoding.Latin1.GetString(buffer).TrimEnd('\0'));
                Console.WriteLine("Recieved " + packets.Last());
            }

            if (packets.Count() > 0)
            {
                string latestPacket = packets.Last(); //later pick the highest frame because can arrive out of order
                JsonElement[] recvData = JsonSerializer.Deserialize<JsonElement[]>(latestPacket);
                recvFNum = recvData[0].GetInt32();
                string[] recvInputs = recvData[1].Deserialize<string[]>();
                int[] recvPos = recvData[2].Deserialize<int[]>();
                int[] recvPoints = recvData[3].Deserialize<int[]>();
                int[] recvBFrames = recvData[4].Deserialize<int[]>();
                char[] recvDirs = recvData[5].Deserialize<char[]>();
                int[] recvAttacks = recvData[6].Deserialize<int[]>();
                bool sameFrame = true;
                sameFrame = sameFrame && Enumerable.SequenceEqual(recvInputs, simHistory[recvFNum].inputs);
                sameFrame = sameFrame && Enumerable.SequenceEqual(recvPos, simHistory[recvFNum].state.positions);
                sameFrame = sameFrame && Enumerable.SequenceEqual(recvPoints, simHistory[recvFNum].state.points);
                sameFrame = sameFrame && Enumerable.SequenceEqual(recvBFrames, simHistory[recvFNum].state.blockFrames);
                sameFrame = sameFrame && Enumerable.SequenceEqual(recvDirs, simHistory[recvFNum].state.dirs);
                sameFrame = sameFrame && Enumerable.SequenceEqual(recvAttacks, simHistory[recvFNum].state.attacks);
                if (!sameFrame)
                {
                    simHistory[recvFNum].state = new GameState(recvPos, recvPoints, recvBFrames, recvDirs, recvAttacks);
                    for(int i = recvFNum + 1; i < simHistory.Count; i++)
                    {
                        string[] correctInputs = new string[pCount];
                        for (int j = 0; j < pCount; j++)
                        {
                            if (j != thisPlayer - 1)
                            {
                                correctInputs[j] = recvInputs[j];
                            }
                        }
                        correctInputs[thisPlayer - 1] = simHistory[i].inputs[thisPlayer - 1];
                        simHistory[i].inputs = correctInputs;
                        simHistory[i].state = NextState(simHistory[i - 1].state, correctInputs, grid);
                    }
                }
            }
            Draw(simHistory.Last().state);
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