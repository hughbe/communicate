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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Communicate.Common;
using Communicate.Server;

namespace Demo
{
    public partial class DemoServer : Form
    {
        public DemoServer()
        {
            InitializeComponent();
        }

        private Server server;

        private void Form1_Load(object sender, EventArgs e)
        {
            TXTRecordList recordList = new TXTRecordList();
            recordList.AddTXTRecord("platform", "windows");
            recordList.AddTXTRecord("publish_time", DateTime.Now.ToString());

            ServerInfo serverInfo = new ServerInfo(Environment.MachineName, 12345, recordList);
            ProtocolInfo protocolInfo = new ProtocolInfo("_ClickBoard", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);

            server = new Server(serverInfo, protocolInfo);

            server.ServerDidPublish += ServerDidPublish;
            server.ServerDidNotPublish += ServerDidNotPublish;
            server.ServerDidUnPublish += ServerDidUnPublish;

            server.ServerDidStartListening += ServerDidStartListening;
            server.ServerDidNotStartListening += ServerDidNotStartListening;
            server.ServerDidStopListening += ServerDidStopListening;

            server.ServerDidStartConnectingToClient += ServerDidStartConnectingToClient;
            server.ServerDidConnectToClient += ServerDidConnectToClient;

            server.ServerDidNotConnectToClient += ServerDidNotConnectToClient;
            server.ServerDidDisconnectFromClient += ServerDidDisconnectFromClient;

            server.ServerDidReceiveDataFromClient += ServerDidReceiveDataFromClient;

            server.Publish();
            server.StartListening();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Stop();
        }

        private void ServerDidPublish(Server server)
        {
            //Console.WriteLine("SERVER: Published");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Published")));
        }

        private void ServerDidNotPublish(Server server, Exception exception)
        {
            //Console.WriteLine("SERVER: Did not publish. Reason: " + exception.ToString());
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Did not publish. Reason: " + exception.ToString())));
        }

        private void ServerDidUnPublish(Server server)
        {
            //Console.WriteLine("SERVER: Unpublished");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Unpublished")));
        }

        private void ServerDidStartListening(Server server)
        {
            //Console.WriteLine("SERVER: Started listening");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Started listening")));
        }

        private void ServerDidNotStartListening(Server server, Exception exception)
        {
            //Console.WriteLine("SERVER: Did not start listening. Reason: " + exception.ToString());
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Did not start listening. Reason: " + exception.ToString())));
        }

        private void ServerDidStopListening(Server server)
        {
            //Console.WriteLine("SERVER: Stopped listening");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Stopped listening")));
        }

        private void ServerDidStartConnectingToClient(Server server, ConnectedClient client)
        {
            //Console.WriteLine("SERVER: Connecting to client");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Connecting to client")));
        }

        private void ServerDidConnectToClient(Server server, ConnectedClient client)
        {
            //Console.WriteLine("SERVER: Connected to client");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Connected to client")));
        }

        private void ServerDidNotConnectToClient(Server server, ConnectedClient client, Exception exception)
        {
            //Console.WriteLine("SERVER: Did not connect to client. Reason: " + exception.ToString());
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Did not connect to client. Reason: " + exception.ToString())));
        }

        private void ServerDidDisconnectFromClient(Server server, ConnectedClient client)
        {
            //Console.WriteLine("SERVER: Disconnected");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Disconnected")));
        }

        private void HandleServerDidReceiveDataFromClient(Server server, ConnectedClient client, byte[] data, string footerContent, DataType dataType)
        {
            if (dataType == DataType.Image)
            {
                Image image = DataSerializer.ByteArrayToImage(data);
                pictureBox1.Image = (Image)image;
            }
            else if (dataType == DataType.File)
            {
                bool useSaveFileDialog = false;

                if (useSaveFileDialog) 
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        string fileName = Path.GetFileNameWithoutExtension(footerContent);
                        string extension = Path.GetExtension(footerContent);
                        if (footerContent.Length > 0)
                        {
                            if (fileName == null)
                            {
                                fileName = footerContent;
                            }

                            string allFilesExtension = "All files (*.*)|*.*";
                            if (extension == null)
                            {
                                extension = allFilesExtension;
                            }
                            else
                            {
                                extension = "(*." + extension + ")|*" + extension + "|" + allFilesExtension;
                            }

                            saveFileDialog.FileName = fileName;
                            saveFileDialog.Filter = extension;
                        }
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllBytes(saveFileDialog.FileName, data);
                        }
                    }
                }
                else
                {
                    string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    filePath = Path.Combine(filePath, footerContent);
                    File.WriteAllBytes(filePath, data);
                }
            }
            else
            {
                string dataInStringForm = Encoding.ASCII.GetString(data, 0, data.Length);
                server.SendStringToClient("Hi client", Encoding.ASCII, client);

                listBox1.Items.Add("SERVER: received data: " + dataInStringForm);
            }
        }

        private void ServerDidReceiveDataFromClient(Server server, ConnectedClient client, byte[] data, string footerContent, DataType dataType)
        {
            Invoke(new Action(() => HandleServerDidReceiveDataFromClient(server, client, data, footerContent, dataType)));
        }

        private void ServerDidSendDataToClient(Server server, byte[] data, ConnectedClient client)
        {
            //Console.WriteLine("SERVER: Sent data to the client");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Sent data to the client")));
        }

        private void ServerDidNotSendDataToClient(Server server, byte[] data, Exception exception, ConnectedClient client)
        {
            //Console.WriteLine("SERVER: Did not send data to the client. Reason: " + exception.ToString());
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Did not send data to the client. Reason: " + exception.ToString())));
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                SendTextBoxTextToAllClients();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendTextBoxTextToAllClients();
        }

        public void SendTextBoxTextToAllClients()
        {
            server.SendStringToAllClients(textBox1.Text);
            textBox1.Text = "";
        }
    }
}
