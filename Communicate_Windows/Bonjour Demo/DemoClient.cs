using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Communicate.Bonjour;
using Communicate;
using System.Net;

namespace Demo.Bonjour
{
    public partial class DemoClient : Form
    {
        private readonly List<SerializationTest> _serializationTestArray = new List<SerializationTest>();

        private BonjourCommunicator _client;

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
            var protocol = new BonjourProtocol("Test");

            _client = new BonjourCommunicator(communicatorInformation, protocol);

            _client.DidUpdateConnectionState += (communicator, eventArgs) =>
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

            _client.DidUpdateServices += delegateCommunicator =>
            {
                var communicator = (BonjourCommunicator) delegateCommunicator;
                devicesListBox.Items.Clear();
                foreach (var service in communicator.DiscoveredServices)
                {
                    devicesListBox.Items.Add(service.Name);
                }
            };

            _client.DidUpdateReceivingData += (communicator, eventArgs) =>
            {
                if (eventArgs.DataComponent == DataComponent.All)
                {
                    if (eventArgs.ActionState == ActionState.Completed)
                    {
                        HandleReceivedData(eventArgs.Data);
                    }
                }
            };

            _client.DidUpdateTxtRecords += (delegateCommunicator, eventArgs) => {
                txtRecordsListView.Items.Clear();

                foreach (var txtRecord in eventArgs.Connection.TxtRecords)
                {
                    txtRecordsListView.Items.Add(txtRecord.Key).SubItems.Add(txtRecord.Value);
                }
                txtRecordsListView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);
                txtRecordsListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            };

            _client.StartSearchingForDevices();
            ToggleAllControls(false);
        }

        private void HandleConnect(BonjourConnection connection)
        {
            _client.SendString("Hi server", connection);
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

        private void HandleReceivedData(Data data) => Console.WriteLine(data.GetString());

        private void devicesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (devicesListBox.SelectedIndex < 0 || devicesListBox.SelectedIndex >= _client.DiscoveredServices.Count)
            {
                return;
            }
            var service = _client.DiscoveredServices[devicesListBox.SelectedIndex];
            _client.ConnectTo(service);
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
            _client.SendString(textBox.Text, null);
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
                    _client.SendFile(filePath, null);
                }
            }
        }

        private void DemoClient_FormClosing(object sender, FormClosingEventArgs e) => _client.Stop();

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
                _client.SendArray(_serializationTestArray, encodedDataType, null);
                serializationArrayCount.Text = "Sent " + _serializationTestArray.Count + " Objects";
                _serializationTestArray.Clear();
            }
            else
            {
                _client.SendEncodedObject(CreateSerializationTextObject(), encodedDataType, null);
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
                _client.ConnectTo(address, (int)manuallyConnectNumbericUpDown.Value);
            }
            else
            {
                MessageBox.Show("Could not parse IP Address");
            }
        }
    }
}