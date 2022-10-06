using System.Net.Sockets;
using System.Net;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        public Graphics()
        {
            InitializeComponent();
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[0];
            Socket server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            server.Connect(new IPEndPoint(address, 12345));
        }

        private void SocketTimer_Tick(object sender, EventArgs e)
        {
            
        }
    }
}