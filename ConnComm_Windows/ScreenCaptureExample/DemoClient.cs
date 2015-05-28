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
using System.Threading;
using System.Drawing.Imaging;
using Communicate.Connections;

namespace ScreenCaptureExample
{
    public partial class DemoClient : Form
    {
        public DemoClient()
        {
            InitializeComponent();
        }

        private Communicator client;
        private void Screen_Load(object sender, EventArgs e)
        {
            ProtocolInfo protocolInfo = new ProtocolInfo("_Test", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);
            CommunicatorInfo clientInfo = new CommunicatorInfo(Environment.MachineName, 12345, null);
            client = new Communicator(protocolInfo, clientInfo);
            client.DidDiscoverServices += ClientDidUpdateServices;
            client.DidConnect += ClientDidConnect;
            client.StartSearching();
        }

        private void ClientDidUpdateServices(Communicator communicator, List<NetService> services)
        {
            listBox1.Items.Clear();
            foreach (NetService service in services)
            {
                listBox1.Items.Add(service.Name);
            }
        }

        private void ClientDidConnect(Communicator communicator, Connection connection)
        {
            Thread backgroundThread = new Thread(new ThreadStart(Screenshot));
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }

        private void Screenshot()
        {
            while (true)
            {
                using (Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                                Screen.PrimaryScreen.Bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bmpScreenCapture))
                    {
                        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                         Screen.PrimaryScreen.Bounds.Y,
                                         0, 0,
                                         bmpScreenCapture.Size,
                                         CopyPixelOperation.SourceCopy);
                    }
                    client.ConnectionManager.SendImage(bmpScreenCapture as Image);
                }
                Thread.Sleep(Convert.ToInt32(1000/15.0));
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < client.SearchingManager.Services.Count)
            {
                NetService service = client.SearchingManager.Services[listBox1.SelectedIndex];
                client.ConnectToService(service);
            }
        }
    }
}
