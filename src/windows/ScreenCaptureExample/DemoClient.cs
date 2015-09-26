using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Communicate;
using Communicate.Bonjour;

namespace ScreenCaptureExample
{
    public partial class DemoClient : Form
    {
        private Thread BackgroundThread { get; set; }
        private BonjourCommunicator Client { get; set; }

        private bool TakingScreenshots { get; set; }

        public DemoClient()
        {
            InitializeComponent();
        }

        private void DemoClient_Load(object sender, EventArgs e)
        {
            var communicatorInformation = new CommunicatorInformation(12345);
            var protocol = new BonjourProtocol("Test");

            Client = new BonjourCommunicator(communicatorInformation, protocol);

            Client.DidUpdateServices += delegateCommunicator =>
            {
                var communicator = (BonjourCommunicator) delegateCommunicator;
                devicesListBox.Items.Clear();
                foreach (var service in communicator.DiscoveredServices)
                {
                    devicesListBox.Items.Add(service.Name);
                }
            };

            Client.DidUpdateConnectionState += (communicator, eventArgs) =>
            {
                var connection = eventArgs.Connection;
                if (connection.State == ConnectionState.Connected)
                {
                    TakingScreenshots = true;
                    BackgroundThread = new Thread(Screenshot) {IsBackground = true};
                    BackgroundThread.Start();
                }
            };

            Client.StartSearchingForDevices();
        }
        
        private void DemoClient_Closing(object sender, FormClosingEventArgs e)
        {
            TakingScreenshots = false;
            BackgroundThread.Abort();
        }

        private void Screenshot()
        {
            while (TakingScreenshots)
            {
                using (var bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height))
                {
                    using (var g = Graphics.FromImage(bmpScreenCapture))
                    {
                        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                            Screen.PrimaryScreen.Bounds.Y,
                            0, 0,
                            bmpScreenCapture.Size,
                            CopyPixelOperation.SourceCopy);
                    }
                    Client.SendImage(bmpScreenCapture, null);
                }
                Thread.Sleep(Convert.ToInt32(1000/15.0));
            }
        }

        private void DevicesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (devicesListBox.SelectedIndex < 0 || devicesListBox.SelectedIndex >= Client.DiscoveredServices.Count)
            {
                return;
            }
            var service = Client.DiscoveredServices[devicesListBox.SelectedIndex];
            Client.ConnectTo(service);
        }
    }
}