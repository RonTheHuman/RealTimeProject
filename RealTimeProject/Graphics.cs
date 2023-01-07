using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        int right = 0, left = 0, block = 0, attack = 0, thisPlayer, curFNum = 1, recvFNum = 0, frameMS = 17, pCount = 1;
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
            int sPort = 12346, cPort = 12345;
            var sAddress = IPAddress.Parse("10.100.102.20");
            var cAddress = IPAddress.Parse("10.100.102.20");
            //address = IPAddress.Parse("172.16.2.167");
            EndPoint clientEP = new IPEndPoint(cAddress, cPort);
            serverEP = new IPEndPoint(sAddress, sPort);
            EndPoint recieveEP = new IPEndPoint(IPAddress.Any, 0);

            clientSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            clientSock.Bind(clientEP);
            clientSock.SendTo(new byte[] { (byte)'a' }, serverEP);
            clientSock.ReceiveFrom(buffer, ref recieveEP);
            thisPlayer = int.Parse(Encoding.Latin1.GetString(buffer).TrimEnd('\0'));
            Console.WriteLine("You are player " + thisPlayer);
            if (pCount == 1)
                simHistory.Add(new Frame(new string[] { "0000" }, new GameState(new int[] { 0 }, new int[] { 0 }, new int[] { -300 }, new char[] { 'r' })));
            else if (pCount == 2)
                simHistory.Add(new Frame(new string[] { "0000", "0000" }, new GameState(new int[] { 0, 100 }, new int[] { 0, 0 }, new int[] { -300, -300 }, new char[] { 'r', 'l' })));
            GameLoopTimer.Interval = frameMS;
            Thread.Sleep(frameMS);
            GameLoopTimer.Enabled = true;
        }

        private void Draw(Dictionary<string, int> gameState)
        {
            Player1Label.Location = new Point(gameState["p1x"], Player1Label.Location.Y);
            Player2Label.Location = new Point(gameState["p2x"], Player2Label.Location.Y);
            ScoreLabel.Text = "Blue score: " + gameState["p1score"] + "\nRed score: " + gameState["p2score"];
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            string curInput = "" + right + left + block + attack;
            clientSock.SendToAsync(Encoding.Latin1.GetBytes(curInput + curFNum), SocketFlags.None, serverEP);
            string[] simInputs = new string[pCount];
            simHistory.Last().inputs.CopyTo(simInputs, 0);
            simInputs[thisPlayer - 1] = curInput;
            simHistory.Add(new Frame(simInputs, CommonCode.NextState(simHistory.Last().state, simInputs, grid)));
            curFNum++;
            List<string> packets = new List<string>();
            while (clientSock.Poll(1, SelectMode.SelectRead))
            {
                byte[] buffer = new byte[1024];
                EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
                clientSock.ReceiveFrom(buffer, ref clientEP);
                packets.Add(Encoding.Latin1.GetString(buffer));
                Console.WriteLine("Recieved " + packets.Last());
            }
            string latestPacket = packets.Last(); //later pick the highest frame because can arrive out of order

            object[] recvData = JsonSerializer.Deserialize<object[]>(latestPacket);
            recvFNum = (int)recvData[0];
            string[] recvInputs = (string[])recvData[1];
            int[] recvPos = (int[])recvData[2];
            int[] recvPoints = (int[])recvData[3];
            int[] recvBFrames = (int[])recvData[4];
            char[] recvDirs = (char[])recvData[5];
            bool sameFrame = true;
            sameFrame = sameFrame && Enumerable.SequenceEqual(recvInputs, simHistory[recvFNum].inputs);
            sameFrame = sameFrame && Enumerable.SequenceEqual(recvPos, simHistory[recvFNum].state.positions);
            sameFrame = sameFrame && Enumerable.SequenceEqual(recvPoints, simHistory[recvFNum].state.points);
            sameFrame = sameFrame && Enumerable.SequenceEqual(recvBFrames, simHistory[recvFNum].state.blockFrames);
            sameFrame = sameFrame && Enumerable.SequenceEqual(recvDirs, simHistory[recvFNum].state.dirs);
            if (!sameFrame)
            {
                simHistory[recvFNum].state = new GameState(recvPos, recvPoints, recvBFrames, recvDirs);
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
                    correctInputs[thisPlayer - 1] = correctInputs[thisPlayer - 1];
                    simHistory[i].inputs = correctInputs;
                    simHistory[i].state = CommonCode.NextState(simHistory[i - 1].state, correctInputs, grid);
                }
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
            TimeLabel.Text = "Time: " + DateTime.Now.ToString("mm.ss.fff");
        }
    }
}