using System;
using System.Windows.Forms;
using Communicate;
using Communicate.Bonjour;

namespace ScreenCaptureExample
{
    public partial class DemoServer : Form
    {
        private BonjourCommunicator Server { get; set; }

        public DemoServer()
        {
            InitializeComponent();
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            var communicatorInformation = new CommunicatorInformation(54321);
            var protocol = new BonjourProtocol("Test");

            Server = new BonjourCommunicator(communicatorInformation, protocol);

            Server.DidUpdateReceivingData += (commuunicator, eventArgs) =>
            {
                if (eventArgs.DataComponent == DataComponent.All)
                {
                    if (eventArgs.ActionState == ActionState.Completed)
                    {
                        HandleReceivedData(eventArgs.Data);
                    }
                }
            };

            Server.PublishOnNetwork();
            Server.StartListeningForConnections();
        }

        private void HandleReceivedData(Data data)
        {
            if (data.GetDataType() != DataType.Image)
            {
                return;
            }
            receivedPictureBox.Image = data.GetImage();
        }
    }
}