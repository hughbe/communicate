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
using Communicate.Connecting;

namespace Demo
{
    public partial class DemoServer : Form
    {
        public DemoServer()
        {
            InitializeComponent();
        }

        private Communicator server;

        private void Form1_Load(object sender, EventArgs e)
        {
            TXTRecordList recordList = new TXTRecordList();
            recordList.AddTXTRecord("platform", "windows");
            recordList.AddTXTRecord("publish_time", DateTime.Now.ToString());
            
            ProtocolInfo protocolInfo = new ProtocolInfo("_Test", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);
            CommunicatorInfo communicatorInfo = new CommunicatorInfo(Environment.MachineName, 62930, recordList);

            server = new Communicator(protocolInfo, communicatorInfo);

            server.DidPublish += ServerDidPublish;
            server.DidNotPublish += ServerDidNotPublish;
            server.DidUnpublish += ServerDidUnPublish;

            server.DidStartListening += ServerDidStartListening;
            server.DidNotStartListening += ServerDidNotStartListening;
            server.DidStopListening += ServerDidStopListening;

            server.DidStartConnecting += ServerDidStartConnecting;
            server.DidConnect += ServerDidConnect;
            server.DidNotConnect += ServerDidNotConnect;
            server.DidDisconnect += ServerDidDisconnect;

            server.DidReceiveData += ServerDidReceiveData;
            server.DidSendData += ServerDidSendData;
            server.DidNotSendData += ServerDidNotSendData;

            server.Publish();
            server.StartListening();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Stop();
        }

        private void ServerDidPublish(Communicator communicator)
        {
            //Console.WriteLine("SERVER: Published");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Published")));
        }

        private void ServerDidNotPublish(Communicator communicator, Exception exception)
        {
            //Console.WriteLine("SERVER: Did not publish. Reason: " + exception.ToString());
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Did not publish. Reason: " + exception.ToString())));
        }

        private void ServerDidUnPublish(Communicator communicator)
        {
            //Console.WriteLine("SERVER: Unpublished");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Unpublished")));
        }

        private void ServerDidStartListening(Communicator communicator)
        {
            //Console.WriteLine("SERVER: Started listening");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Started listening")));
        }

        private void ServerDidNotStartListening(Communicator communicator, Exception exception)
        {
            //Console.WriteLine("SERVER: Did not start listening. Reason: " + exception.ToString());
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Did not start listening. Reason: " + exception.ToString())));
        }

        private void ServerDidStopListening(Communicator communicator)
        {
            //Console.WriteLine("SERVER: Stopped listening");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Stopped listening")));
        }

        private void ServerDidStartConnecting(Communicator communicator, Connection connection)
        {
            //Console.WriteLine("SERVER: Connecting to client");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Connecting to client")));
        }

        private void ServerDidConnect(Communicator communicator, Connection connection)
        {
            //Console.WriteLine("SERVER: Connected to client");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Connected to client")));
        }

        private void ServerDidNotConnect(Communicator communicator, Connection connection, Exception exception)
        {
            //Console.WriteLine("SERVER: Did not connect to client. Reason: " + exception.ToString());
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Did not connect to client. Reason: " + exception.ToString())));
        }

        private void ServerDidDisconnect(Communicator communicator, Connection connection)
        {
            //Console.WriteLine("SERVER: Disconnected");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Disconnected")));
        }

        private void HandleServerDidReceiveDataFromClient(Communicator communicator, Connection connection, CommunicationData data)
        {
            if (data.GetDataType() == CommunicationDataType.Image)
            {
                Image image = data.ToImage();
                pictureBox1.Image = (Image)image;
            }
            else if (data.GetDataType()  == CommunicationDataType.File)
            {
                bool useSaveFileDialog = false;

                if (useSaveFileDialog) 
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        string footerContent = data.DataHeader.FileName;
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
                            File.WriteAllBytes(saveFileDialog.FileName, data.DataContent.GetBytes());
                        }
                    }
                }
                else
                {
                    string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    filePath = Path.Combine(filePath, data.DataHeader.FileName);
                    File.WriteAllBytes(filePath, data.DataContent.GetBytes());
                }
            }
            else 
            {
                string dataInStringForm = Encoding.ASCII.GetString(data.DataContent.GetBytes(), 0, data.DataInfo.ContentLength);
                //server.SendString("Hi client", Encoding.ASCII, connection);

                listBox1.Items.Add("SERVER: received data: " + dataInStringForm);
            }
        }

        private void ServerDidReceiveData(Communicator communicator, Connection connection, CommunicationData data)
        {
            Invoke(new Action(() => HandleServerDidReceiveDataFromClient(server, connection, data)));
        }

        private void ServerDidSendData(Communicator communicator, Connection connection, CommunicationData data)
        {
            //Console.WriteLine("SERVER: Sent data to the client");
            listBox1.Invoke(new Action(() => listBox1.Items.Add("SERVER: Sent data to the client")));
        }

        private void ServerDidNotSendData(Communicator communicator, Connection connection, CommunicationData data, Exception exception)
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
            server.ConnectionManager.SendString(textBox1.Text);
            textBox1.Text = "";
        }
    }
}
