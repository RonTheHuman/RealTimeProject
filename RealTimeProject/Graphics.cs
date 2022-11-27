using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        bool gridMovement = true;
        bool rightPressed = false;
        bool leftPressed = false;
        Task bulletTimeout = Task.Factory.StartNew(() => Thread.Sleep(1));
        int thisPlayer;

        byte[] buffer = new byte[1024];
        Task<int> recvTask, recvEchoTask;
        Socket server, serverEcho;
        public Graphics()
        {
            InitializeComponent();
        }
        private void Graphics_Load(object sender, EventArgs e)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[1];
            address = IPAddress.Parse("172.16.2.167");
            //address = IPAddress.Parse("10.100.102.20");
            Thread.Sleep(2);
            server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            server.Connect(new IPEndPoint(address, 12345));
            serverEcho = new Socket(SocketType.Stream, ProtocolType.Tcp);
            serverEcho.Connect(new IPEndPoint(address, 12346));
            server.Receive(buffer);
            thisPlayer = int.Parse(Encoding.Latin1.GetString(buffer));
            recvTask = server.ReceiveAsync(buffer, new SocketFlags());
            recvEchoTask = serverEcho.ReceiveAsync(new byte[1], new SocketFlags());
            server.Send(Encoding.Latin1.GetBytes("MoveRight,"));
        }

        private void UpdateGraphics(Dictionary<string, int> gameState)
        {
            Player1Label.Location = new Point(gameState["p1x"], Player1Label.Location.Y);
            Player2Label.Location = new Point(gameState["p2x"], Player2Label.Location.Y);
            ScoreLabel.Text = "Blue score: " + gameState["p1score"] + "\nRed score: " + gameState["p2score"];
        }

        private void SocketTimer_Tick(object sender, EventArgs e)
        {
            if (recvEchoTask.IsCompleted)
            {
                Console.WriteLine(DateTime.Now.ToString("mm.ss.fff") + "| " + "[sent echo]");
                serverEcho.Send(new byte[1]);
                recvEchoTask = serverEcho.ReceiveAsync(new byte[1], new SocketFlags());
            }
            if (recvTask.IsCompleted)
            {
                //Console.WriteLine("[{0}]", string.Join(", ", buffer));
                string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0')[..recvTask.Result];
                while (data[data.Length - 1] != '}')
                {
                    recvTask = server.ReceiveAsync(buffer, new SocketFlags());
                    data += Encoding.Latin1.GetString(buffer).TrimEnd('\0')[..recvTask.Result];
                }
                data = data.Substring(data.LastIndexOf('{'));
                //Console.WriteLine("Data:" + data);
                UpdateGraphics(JsonSerializer.Deserialize<Dictionary<string, int>>(data));

                recvTask = server.ReceiveAsync(buffer, new SocketFlags());
            }
            if (rightPressed)
            {
                Console.Write(DateTime.Now.ToString("mm.ss.fff") + "| ");
                server.Send(Encoding.Latin1.GetBytes("MoveRight,"));
                Console.WriteLine("Sent MoveRight");
            }
            if (leftPressed)
            {
                Console.Write(DateTime.Now.ToString("mm.ss.fff") + "| ");
                server.Send(Encoding.Latin1.GetBytes("MoveLeft,"));
                Console.WriteLine("Sent MoveLeft");
            }
            if (bulletTimeout.IsCompleted)
            {
                if (thisPlayer == 1)
                    Bullet1Label.Visible = false;
                else
                    Bullet2Label.Visible = false;
            }
        }

        private void Graphics_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                if (gridMovement)
                {
                    server.Send(Encoding.Latin1.GetBytes("MoveRight,"));
                    Console.Write(DateTime.Now.ToString("mm.ss.fff") + "| ");
                    Console.WriteLine("Sent MoveRight");
                }
                else
                    rightPressed = true;
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (gridMovement)
                {
                    server.Send(Encoding.Latin1.GetBytes("MoveLeft,"));
                    Console.Write(DateTime.Now.ToString("mm.ss.fff") + "| ");
                    Console.WriteLine("Sent MoveLeft");
                }
                else
                    leftPressed = true;
            }
            else if(e.KeyCode == Keys.Space)
            {
                server.Send(Encoding.Latin1.GetBytes("Shoot,"));
                Console.Write(DateTime.Now.ToString("mm.ss.fff") + "| ");
                Console.WriteLine("Sent Shoot");
                //client simulation
                if (thisPlayer == 1)
                {
                    Bullet1Label.Location = new Point(Player1Label.Location.X + 18, Bullet1Label.Location.Y);
                    Bullet1Label.Visible = true;
                }
                if (thisPlayer == 2)
                {
                    Bullet2Label.Location = new Point(Player2Label.Location.X + 18, Bullet2Label.Location.Y);
                    Bullet2Label.Visible = true;
                }
                if (bulletTimeout.IsCompleted)
                {
                    bulletTimeout = Task.Factory.StartNew(() => Thread.Sleep(90));
                }
            }
            else if(e.KeyCode == Keys.G)
            {
                server.Send(Encoding.Latin1.GetBytes("Grid,"));
                gridMovement = !gridMovement;
                Console.Write(DateTime.Now.ToString("mm.ss.fff") + "| ");
                Console.WriteLine("Sent Grid");

            }
            else if (e.KeyCode == Keys.L)
            {
                server.Send(Encoding.Latin1.GetBytes("LagComp,"));
                Console.Write(DateTime.Now.ToString("mm.ss.fff") + "| ");
                Console.WriteLine("Sent LagComp");
            }

        }

        private void Graphics_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                rightPressed = false;
            }
            if (e.KeyCode == Keys.Left)
            {
                leftPressed = false;
            }
        }

        private void TimeTimer_Tick(object sender, EventArgs e)
        {
            TimeLabel.Text = "Time: " + DateTime.Now.ToString("mm.ss.fff");
        }
    }
}