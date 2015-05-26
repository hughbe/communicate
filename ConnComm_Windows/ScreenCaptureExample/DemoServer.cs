using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Communicate;
using Communicate.Client;
using Communicate.Common;
using ZeroconfService;
using Communicate.Server;

namespace ScreenCaptureExample
{
    public partial class DemoServer : Form
    {
        public DemoServer()
        {
            InitializeComponent();
        }

        private Server server;
        private void Screen_Load(object sender, EventArgs e)
        {
            ServerInfo serverInfo = new ServerInfo(Environment.MachineName, 12345, null);
            ProtocolInfo protocolInfo = new ProtocolInfo("_ClickBoard", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);

            server = new Server(serverInfo, protocolInfo);
            server.ServerDidReceiveDataFromClient += ServerDidReceiveDataFromClient;

            server.PublishAndListen();
        }

        private void ServerDidReceiveDataFromClient(Server server, ConnectedClient client, CommunicationData data)
        {
            if (data.GetDataType() == CommunicationDataType.Image)
            {
                Image image = data.ToImage();
                pictureBox1.Image = image;
            }
        }
    }
}
