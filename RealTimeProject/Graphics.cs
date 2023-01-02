using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        int right = 0, left = 0, block = 0, attack = 0, thisPlayer, fNum = 1, frameMS = 500;
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
            GameLoopTimer.Enabled = true;
            GameLoopTimer.Interval = frameMS;
        }

        private void UpdateGraphics(Dictionary<string, int> gameState)
        {
            Player1Label.Location = new Point(gameState["p1x"], Player1Label.Location.Y);
            Player2Label.Location = new Point(gameState["p2x"], Player2Label.Location.Y);
            ScoreLabel.Text = "Blue score: " + gameState["p1score"] + "\nRed score: " + gameState["p2score"];
        }

        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            clientSock.SendTo(Encoding.Latin1.GetBytes("" + fNum + right + left + block + attack), serverEP);
            fNum++;
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