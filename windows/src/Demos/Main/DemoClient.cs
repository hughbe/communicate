using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Communicate.Bonjour;
using Communicate;
using System.Net;
using System.IO;
using System.Linq;

namespace Demo.Bonjour
{
    public partial class DemoClient : Form
    {
        private readonly List<SerializationTest> _serializationTestArray = new List<SerializationTest>();

        private BonjourCommunicator Client { get; set; }

        public DemoClient()
        {
            InitializeComponent();
        }

        private void DemoClient_Load(object sender, EventArgs e)
        {
            int port = 54321;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    ipAddressLabel.Text = ip.ToString() + ":" + port;
                }
            }

            var communicatorInformation = new CommunicatorInformation(port);
            var protocol = new Protocol("Test");

            Client = new BonjourCommunicator(communicatorInformation, protocol);

            Client.DidUpdateConnectionState += (communicator, eventArgs) =>
            {
                var connection = eventArgs.Connection;
                Console.WriteLine(connection.State);
                switch (connection.State)
                {
                    case ConnectionState.NotConnected:
                        break;
                    case ConnectionState.Connecting:
                        break;
                    case ConnectionState.Resolving:
                        break;
                    case ConnectionState.Resolved:
                        break;
                    case ConnectionState.Connected:
                        HandleConnect(connection);
                        break;
                    case ConnectionState.Error:
                        break;
                    case ConnectionState.Disconnected:
                        HandleDisconnect();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            Client.DidUpdateServices += (communicator, eventArgs) =>
            {
                devicesListBox.Items.Clear();
                foreach (var service in Client.DiscoveredServices.ToList())
                {
                    devicesListBox.Items.Add(service.Information.Name);
                }
            };

            Client.DidUpdateReceivingData += (communicator, eventArgs) =>
            {
                if (eventArgs.DataComponent == DataComponent.All)
                {
                    if (eventArgs.ActionState == ActionState.Completed)
                    {
                        HandleReceivedData(eventArgs.Data);
                    }
                }
            };

            Client.DidUpdateTxtRecords += (communicator, eventArgs) => 
            {
                txtRecordsListView.Items.Clear();

                foreach (var txtRecord in eventArgs.Connection.TxtRecords.ToList())
                {
                    txtRecordsListView.Items.Add(txtRecord.Key).SubItems.Add(txtRecord.Value);
                }
                txtRecordsListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                txtRecordsListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            };

            Client.StartSearchingForDevices();
            ToggleAllControls(false);
        }

        private void HandleConnect(Connection connection)
        {
            Client.SendData(new CommunicationData().WithString("Hi server"), connection);
            ToggleAllControls(true);
        }

        private void HandleDisconnect() => ToggleAllControls(false);

        private void ToggleAllControls(bool enabled)
        {
            Invoke((MethodInvoker) delegate
            {
                devicesListBox.Enabled = !enabled;

                manuallyConnectTextBox.Enabled = !enabled;
                manuallyConnectNumbericUpDown.Enabled = !enabled;
                manuallyConnectButton.Enabled = !enabled;                            

                textBox.Enabled = enabled;
                sendTextButton.Enabled = enabled;
                sendFileButton.Enabled = enabled;

                serializationTextBox.Enabled = enabled;
                serializationCheckBox.Enabled = enabled;
                serializationNumericUpDown.Enabled = enabled;

                sendJsonButton.Enabled = enabled;
                sendXmlButton.Enabled = enabled;
                sendSoapButton.Enabled = enabled;
                sendBinaryButton.Enabled = enabled;

                addToArrayButton.Enabled = enabled;
                sendArrayCheckBox.Enabled = enabled;
            });
        }

        private void HandleReceivedData(CommunicationData data) => Console.WriteLine(data.GetString());

        private void devicesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (devicesListBox.SelectedIndex < 0 || devicesListBox.SelectedIndex >= Client.DiscoveredServices.Count)
            {
                return;
            }
            var service = Client.DiscoveredServices[devicesListBox.SelectedIndex];
            Client.ConnectTo(service);
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendTextButton_Click(null, null);
            }
        }

        private void sendTextButton_Click(object sender, EventArgs e)
        {
            Client.SendData(new CommunicationData().WithString(textBox.Text), null);
            textBox.Text = "";
        }

        private void sendFileButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Title = "Please select a file to send"
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                foreach (var filePath in dialog.FileNames)
                {
                    Client.SendData(
                        new CommunicationData()
                        .WithFilePath(filePath)
                        .WithName(Path.GetFileName(filePath))
                        .WithPath(filePath), null);
                }
            }
        }

        private void DemoClient_FormClosing(object sender, FormClosingEventArgs e) => Client.StopAll();

        private SerializationTest CreateSerializationTextObject()
        {
            var test = new SerializationTest
            {
                Property1 = serializationTextBox.Text,
                Property2 = serializationCheckBox.Checked,
                Property3 = Convert.ToInt32(serializationNumericUpDown.Value)
            };
            return test;
        }

        private void SendSerializationTest(EncodedDataType encodedDataType)
        {
            if (sendArrayCheckBox.Checked)
            {
                Client.SendData(new CommunicationData().WithList(_serializationTestArray, encodedDataType), null);
                serializationArrayCount.Text = "Sent " + _serializationTestArray.Count + " Objects";
                _serializationTestArray.Clear();
            }
            else
            {
                Client.SendData(new CommunicationData().WithObject(CreateSerializationTextObject(), encodedDataType), null);
            }
        }

        private void sendJsonButton_Click(object sender, EventArgs e) => SendSerializationTest(EncodedDataType.Json);

        private void sendXmlButton_Click(object sender, EventArgs e) => SendSerializationTest(EncodedDataType.Xml);

        private void sendSoapButton_Click(object sender, EventArgs e) => SendSerializationTest(EncodedDataType.Soap);

        private void sendBinaryButton_Click(object sender, EventArgs e) => SendSerializationTest(EncodedDataType.Binary);

        private void addToArrayButton_Click(object sender, EventArgs e)
        {
            _serializationTestArray.Add(CreateSerializationTextObject());
            serializationArrayCount.Text = _serializationTestArray.Count + " Objects";
        }

        private void sendArrayCheckBox_CheckedChanged(object sender, EventArgs e)
            => sendSoapButton.Enabled = !sendArrayCheckBox.Checked;

        private void manuallyConnectButton_Click(object sender, EventArgs e)
        {
            IPAddress address;
            if (IPAddress.TryParse(manuallyConnectTextBox.Text, out address))
            {
                Client.ConnectTo(address, (int)manuallyConnectNumbericUpDown.Value);
            }
            else
            {
                MessageBox.Show("Could not parse IP Address");
            }
        }
    }
}