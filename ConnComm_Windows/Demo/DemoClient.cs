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
using ZeroconfService;
using Communicate.Common;
using System.IO;
using System.Threading;
using Communicate.Connections;

namespace Demo
{
    public partial class DemoClient : Form
    {
        public DemoClient()
        {
            InitializeComponent();
        }

        private Communicator client;

        private void DemoClient_Load(object sender, EventArgs e)
        {
            ProtocolInfo protocolInfo = new ProtocolInfo("_Test", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);
            CommunicatorInfo communicatorInfo = new CommunicatorInfo(Environment.MachineName, 12345, null);

            client = new Communicator(protocolInfo, communicatorInfo);
            client.DidStartSearching += DidStartSearching;
            client.DidDiscoverServices += DidUpdateServices;
            client.DidNotStartSearching += DidNotSearch;

            client.DidStartConnecting += DidStartConnecting;
            client.DidConnect += DidConnect;
            client.DidNotConnect += DidNotConnect;
            client.DidDisconnect += DidDisconnect;

            client.DidReceiveData += DidReceiveData;
            client.DidSendData += DidSendData;
            client.DidNotSendData += DidNotSendData;

            client.StartSearching();
            textBox1.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = false;
        }

        private void DidStartSearching(Communicator communicator) 
        {
            //Console.WriteLine("CLIENT: Started Searching");
        }

        private void DidNotSearch(Communicator communicator, Exception exception)
        {
            //Console.WriteLine("CLIENT: Failed to search. Reason: " + exception.ToString());
        }
        
        private void DidUpdateServices(Communicator communicator, List<NetService>services)
        {
            listBox1.Items.Clear();
            foreach (NetService service in services)
            {
                //Console.WriteLine(service.Name);
                listBox1.Items.Add(service.Name);
            }
        }

        private void DidStartConnecting(Communicator communicator, Connection connection)
        {
            //Console.WriteLine("CLIENT: Connecting to server");
        }

        private void DidConnect(Communicator communicator, Connection connection)
        {
            //Console.WriteLine("CLIENT: Connected to server");
            client.ConnectionManager.SendString("Hi server", Encoding.ASCII);
            textBox1.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = true;
        }

        private void DidNotConnect(Communicator communicator, Connection connection, Exception exception)
        {
            //Console.WriteLine("CLIENT: Did not connect to server. Reason: " + exception.ToString());
        }

        private void DidDisconnect(Communicator communicator, Connection connection)
        {
            Console.WriteLine("CLIENT: Disconnected");
            Invoke(new Action(() => textBox1.Enabled = false));
            Invoke(new Action(() => button1.Enabled = false));
            Invoke(new Action(() => button3.Enabled = false));
        }

        private void DidReceiveData(Communicator communicator, Connection connection, CommunicationData data)
        {
            Console.WriteLine("CLIENT: received data: " + Encoding.ASCII.GetString(data.DataContent.GetBytes()));
        }

        private void DidSendData(Communicator communicator, Connection connection, CommunicationData data)
        {
            //Console.WriteLine("CLIENT: Sent data");
        }

        private void DidNotSendData(Communicator communicator, Connection connection, CommunicationData data, Exception exception)
        {
            //Console.WriteLine("CLIENT: Failed to send: " + Encoding.ASCII.GetString(data) + "; Reason: " + exception.ToString());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < client.SearchingManager.Services.Count) 
            {
                NetService service = client.SearchingManager.Services[listBox1.SelectedIndex];
                client.ConnectToService(service);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SendTextBoxData();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendTextBoxData();
            }
        }

        public void SendTextBoxData() 
        {
            client.ConnectionManager.SendString(textBox1.Text);
            textBox1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = @"C:\";
            dialog.Title = "Please select a file to send";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in dialog.FileNames) 
                {
                    client.ConnectionManager.SendFile(filePath);
                }
            }  
        }

        private void DemoClient_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Stop();
        }
    }
}
