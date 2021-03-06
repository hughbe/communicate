﻿using System;
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
            var protocol = new CommunicatorProtocol("Test");

            Server = new BonjourCommunicator(communicatorInformation, protocol);

            Server.DidUpdateReceivingData += (commuunicator, eventArgs) =>
            {
                if (eventArgs.Component == DataComponent.All)
                {
                    if (eventArgs.DataState == ActionState.Completed)
                    {
                        HandleReceivedData(eventArgs.Data);
                    }
                }
            };

            Server.PublishOnNetwork();
            Server.StartListeningForConnections();
        }

        private void HandleReceivedData(CommunicationData data)
        {
            if (data.DataType != DataType.Image)
            {
                return;
            }
            receivedPictureBox.Image = data.GetImage();
        }
    }
}