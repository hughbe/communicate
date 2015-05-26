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
using System.Threading;
using System.Drawing.Imaging;

namespace ScreenCaptureExample
{
    public partial class DemoClient : Form
    {
        public DemoClient()
        {
            InitializeComponent();
        }

        private Client client;
        private void Screen_Load(object sender, EventArgs e)
        {
            ProtocolInfo protocolInfo = new ProtocolInfo("_ClickBoard", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);
            ClientInfo clientInfo = new ClientInfo(12345);
            client = new Client(protocolInfo, clientInfo);
            client.ClientDidUpdateServices += ClientDidUpdateServices;
            client.ClientDidConnect += ClientDidConnect;
            client.Search();
        }
        
        private void ClientDidUpdateServices(Client client)
        {
            listBox1.Items.Clear();
            foreach (NetService service in client.Services)
            {
                listBox1.Items.Add(service.Name);
            }
        }

        private void ClientDidConnect(Client client)
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
                    client.SendImage(bmpScreenCapture as Image);
                }
                Thread.Sleep(Convert.ToInt32(1000/15.0));
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < client.Services.Count)
            {
                NetService service = client.Services[listBox1.SelectedIndex];
                client.ConnectToService(service);
            }
        }
    }
}
