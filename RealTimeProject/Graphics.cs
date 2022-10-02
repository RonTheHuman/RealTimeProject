using System.Net.Sockets;
using System.Net;

namespace RealTimeProject
{
    public partial class Graphics : Form
    {
        public Graphics()
        {
            InitializeComponent();
            Socket server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress address = ipHost.AddressList[0];
            //address = IPAddress.Any;
            server.Connect(new IPEndPoint(address, 0));
        }

        private void SocketTimer_Tick(object sender, EventArgs e)
        {
            
        }
    }
}