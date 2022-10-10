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
        byte[] data = new byte[64];
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
        }

        private void SocketTimer_Tick(object sender, EventArgs e)
        {
            server.ReceiveAsync(data, new SocketFlags());
            string xStr = Encoding.Latin1.GetString(data).TrimEnd('\0');
            if (xStr != "")
            {
                Console.Write("Data:");
                Console.WriteLine(xStr);
                TestLabel.Location = new Point(int.Parse(xStr), TestLabel.Location.Y);
            }
            if (RightPressed)
            {
                server.Send(Encoding.Latin1.GetBytes("MoveRight"));
                Console.WriteLine("Sent MoveRight");
            }
            if (LeftPressed)
            {
                server.Send(Encoding.Latin1.GetBytes("MoveLeft"));
                Console.WriteLine("Sent MoveLeft");
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