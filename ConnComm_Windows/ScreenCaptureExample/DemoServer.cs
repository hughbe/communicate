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
using Communicate.Common;
using ZeroconfService;
using Communicate.Connecting;

namespace ScreenCaptureExample
{
    public partial class DemoServer : Form
    {
        public DemoServer()
        {
            InitializeComponent();
        }

        private Communicator server;
        private void Screen_Load(object sender, EventArgs e)
        {
            CommunicatorInfo communicatorInfo = new CommunicatorInfo(Environment.MachineName, 12345, null);
            ProtocolInfo protocolInfo = new ProtocolInfo("_Test", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);

            server = new Communicator(protocolInfo, communicatorInfo);
            server.DidReceiveData += DidReceiveData;

            server.PublishAndStartListening();
        }

        private void DidReceiveData(Communicator communicator, Connection connection, CommunicationData data)
        {
            if (data.GetDataType() == CommunicationDataType.Image)
            {
                Image image = data.ToImage();
                pictureBox1.Image = image;
            }
        }
    }
}
