using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;
using System.Text.Json;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        bool RightPressed = false;
        bool LeftPressed = false;

        byte[] buffer = new byte[64];
        Task<int> recvTask;
        Task bulletTimeout = Task.Factory.StartNew(() => Thread.Sleep(1));
        Socket server;
        public Graphics()
        {
            InitializeComponent();
        }
        private void Graphics_Load(object sender, EventArgs e)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[1];

            server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            server.Connect(new IPEndPoint(address, 12345));
            recvTask = server.ReceiveAsync(buffer, new SocketFlags());
        }

        private void UpdateGraphics(Dictionary<string, int> gameState)
        {
            Player1Label.Location = new Point(gameState["p1x"], Player1Label.Location.Y);
            Player2Label.Location = new Point(gameState["p2x"], Player2Label.Location.Y);
        }

        private void SocketTimer_Tick(object sender, EventArgs e)
        {
            if (recvTask.IsCompleted)
            {
                //Console.WriteLine("[{0}]", string.Join(", ", buffer));
                string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0').Substring(0, recvTask.Result);
                while (data[data.Length - 1] != '}')
                {
                    recvTask = server.ReceiveAsync(buffer, new SocketFlags());
                    data += Encoding.Latin1.GetString(buffer).TrimEnd('\0').Substring(0, recvTask.Result);
                }
                data = data.Substring(data.LastIndexOf('{'));
                Console.WriteLine("Data:" + data);
                UpdateGraphics(JsonSerializer.Deserialize<Dictionary<string, int>>(data));

                recvTask = server.ReceiveAsync(buffer, new SocketFlags());
            }
            if (RightPressed)
            {
                server.Send(Encoding.Latin1.GetBytes("MoveRight,"));
                Console.WriteLine("Sent MoveRight");
            }
            if (LeftPressed)
            {
                server.Send(Encoding.Latin1.GetBytes("MoveLeft,"));
                Console.WriteLine("Sent MoveRight");
            }
            if (bulletTimeout.IsCompleted)
            {
                Bullet1Label.Visible = false;
            }
        }

        private void Graphics_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                RightPressed = true;
            }
            if (e.KeyCode == Keys.Left)
            {
                LeftPressed = true;
            }
            if (e.KeyCode == Keys.Space)
            {
                server.Send(Encoding.Latin1.GetBytes("Shoot,"));
                Bullet1Label.Location = new Point(Player1Label.Location.X + 18, Bullet1Label.Location.Y);
                Bullet1Label.Visible = true;
                if (bulletTimeout.IsCompleted)
                {
                    bulletTimeout = Task.Factory.StartNew(() => Thread.Sleep(90));
                }
            }
        }

        private void Graphics_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Right)
            {
                RightPressed = false;
            }
            if (e.KeyCode == Keys.Left)
            {
                LeftPressed = false;
            }
        }
    }
}