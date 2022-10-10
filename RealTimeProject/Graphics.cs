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
        Socket server;
        public Graphics()
        {
            InitializeComponent();
        }
        private void Graphics_Load(object sender, EventArgs e)
        {
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[0];
            server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            server.Connect(new IPEndPoint(address, 12345));
        }

        private void SocketTimer_Tick(object sender, EventArgs e)
        {
            if (RightPressed)
            {
                server.Send(Encoding.Latin1.GetBytes("MoveRight"));
            }
            if (RightPressed)
            {
                server.Send(Encoding.Latin1.GetBytes("MoveLeft"));
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