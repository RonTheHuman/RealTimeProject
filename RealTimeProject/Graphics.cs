using System.Net.Sockets;
using System.Net;
using System.Windows.Input;
using System.Text;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        bool RightPressed = false;
        bool LeftPressed = false;
        byte[] buffer = new byte[64];
        Task<int> recvTask;
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

        private void SocketTimer_Tick(object sender, EventArgs e)
        {
            if (recvTask.IsCompleted)
            {
                //Console.WriteLine("[{0}]", string.Join(", ", buffer));
                string data = Encoding.Latin1.GetString(buffer).TrimEnd('\0');
                Console.Write("Data:");
                Console.WriteLine(data);
                TestLabel.Location = new Point(int.Parse(data.Substring(0, recvTask.Result)), TestLabel.Location.Y);
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