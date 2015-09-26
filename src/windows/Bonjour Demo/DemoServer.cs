using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using Communicate;
using Communicate.Bonjour;
using System.Net;
using System.Net.Sockets;

namespace Demo.Bonjour
{
    public partial class DemoServer : Form
    {
        private BonjourCommunicator _server;

        public DemoServer()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var port = 12345;
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    AddHistory("Running server on " + ip.ToString() + ":" + port);
                    break;
                }
            }

            var txtRecords = new BonjourTxtRecords(new List<TxtRecord>
            {
                new TxtRecord("platform", "windows"),
                new TxtRecord("publish_time", DateTime.Now.ToString(CultureInfo.InvariantCulture))
            });

            var communicatorInformation = new CommunicatorInformation(port);
            var protocol = new BonjourProtocol("Test");

            _server = new BonjourCommunicator(communicatorInformation, protocol);

            _server.UpdateTxtRecords(txtRecords);

            _server.DidUpdatePublishedState += delegateCommunicator =>
            {
                var communicator = (BonjourCommunicator) delegateCommunicator;
                switch (communicator.PublishedState)
                {
                    case State.Ready:
                        break;
                    case State.Starting:
                        break;
                    case State.Started:
                        AddHistory("Published");
                        break;
                    case State.Error:
                        AddHistory("Error Publishing: " + communicator.PublishingException);
                        break;
                    case State.Stopped:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            _server.DidUpdateListeningState += delegateCommunicator =>
            {
                var communicator = (BonjourCommunicator) delegateCommunicator;
                switch (communicator.ListeningState)
                {
                    case State.Ready:
                        break;
                    case State.Starting:
                        break;
                    case State.Started:
                        AddHistory("Listening");
                        break;
                    case State.Error:
                        AddHistory("Not Listening: " + communicator.ListeningException);
                        break;
                    case State.Stopped:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            _server.DidUpdateConnectionState += (communicator, eventArgs) =>
            {
                var connection = eventArgs.Connection;
                switch (connection.State)
                {
                    case ConnectionState.NotConnected:
                        break;
                    case ConnectionState.Connecting:
                        AddHistory("Connecting to Client");
                        break;
                    case ConnectionState.Resolving:
                        break;
                    case ConnectionState.Resolved:
                        break;
                    case ConnectionState.Connected:
                        AddHistory("Connected to Client");
                        break;
                    case ConnectionState.Error:
                        AddHistory("Did Not Connect to Client: " + connection.ConnectionException);
                        break;
                    case ConnectionState.Disconnected:
                        AddHistory("Disconnected from Client");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            _server.DidUpdateTxtRecords += (communicator, eventArgs) =>
            {
                Console.WriteLine("Received TxtRecords");
                Console.WriteLine(eventArgs.Connection.TxtRecords);
            };

            _server.DidUpdateReceivingData += (communicator, eventArgs) =>
            {
                if (eventArgs.DataComponent == DataComponent.Content)
                {
                    if (eventArgs.ActionState == ActionState.Updating)
                    {
                        receivingProgressBar.Invoke(
                            (MethodInvoker) (() => receivingProgressBar.Value = (int) (eventArgs.Progress*100)));
                    }
                }
                else if (eventArgs.DataComponent == DataComponent.All)
                {
                    if (eventArgs.ActionState == ActionState.Completed)
                    {
                        receivingProgressBar.Invoke(
                            (MethodInvoker) (() => HandleReceivedData(eventArgs.Data)));
                    }
                }
            };

            _server.PublishOnNetwork();
            _server.StartListeningForConnections();
        }

        private void AddHistory(string history)
            => historyListBox.Invoke((MethodInvoker) (() => historyListBox.Items.Add(history)));

        private void HandleReceivedData(Data data)
        {
            receivingProgressBar.Value = 100;
            if (data.GetDataType() == DataType.Image)
            {
                var image = data.GetImage();
                receivedPictureBox.Image = image;
            }
            else if (data.GetDataType() == DataType.File)
            {
                if (saveFilesCheckBox.Checked)
                {
                    var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    filePath = Path.Combine(filePath, data.Header.FileName);
                    File.WriteAllBytes(filePath, data.Content);
                }
                else
                {
                    using (var saveFileDialog = new SaveFileDialog())
                    {
                        var footerContent = data.Header.FileName;
                        var fileName = Path.GetFileNameWithoutExtension(footerContent);
                        var extension = Path.GetExtension(footerContent) ?? "All files (*.*)|*.*";
                        if (footerContent.Length > 0)
                        {
                            if (fileName == null)
                            {
                                fileName = footerContent;
                            }

                            saveFileDialog.FileName = fileName;
                            saveFileDialog.Filter = extension;
                        }
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllBytes(saveFileDialog.FileName, data.Content);
                        }
                    }
                }
            }
            else if (data.GetDataType().IsObject)
            {
                var testObject = data.GetObject<SerializationTest>();
                property1Label.Text = "Property 1: " + testObject.Property1;
                property2Label.Text = "Property 2: " + testObject.Property2;
                property3Label.Text = "Property 3: " + testObject.Property3;
            }
            else if (data.GetDataType().IsArray)
            {
                receivedArrayListBox.Items.Clear();
                IList<SerializationTest> testObjects = data.GetArray<SerializationTest>();
                foreach (var testObject in testObjects)
                {
                    receivedArrayListBox.Items.Add(testObject.ToString());
                }
            }
            else
            {
                var receivedString = data.GetString();
                AddHistory("SERVER: received data: " + receivedString);
            }
        }

        private void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                sendTextButton_Clicked(null, null);
            }
        }

        private void sendTextButton_Clicked(object sender, EventArgs e)
        {
            _server.SendString(textBox.Text, null);
            textBox.Text = "";
        }

        private void DemoServer_FormClosing(object sender, FormClosingEventArgs e) => _server.Stop();
    }
}