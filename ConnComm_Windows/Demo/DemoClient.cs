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
using Communicate.Client;
using Communicate.Common;
using System.IO;
using System.Threading;

namespace Demo
{
    public partial class DemoClient : Form
    {
        public DemoClient()
        {
            InitializeComponent();
        }

        private Client client;

        private void DemoClient_Load(object sender, EventArgs e)
        {        
            ProtocolInfo protocolInfo = new ProtocolInfo("_ClickBoard", TransportProtocolType.TCP, ProtocolInfo.ProtocolDomainLocal);
            ClientInfo clientInfo = new ClientInfo(12345);
            client = new Client(protocolInfo, clientInfo);
            client.ClientDidStartSearching += ClientDidStartSearching;
            client.ClientDidNotSearch += ClientDidNotSearch;
            client.ClientDidUpdateServices += ClientDidUpdateServices;

            client.ClientDidStartConnecting += ClientDidStartConnecting;
            client.ClientDidConnect += ClientDidConnect;
            client.ClientDidNotConnect += ClientDidNotConnect;
            client.ClientDidDisconnect += ClientDidDisconnect;

            client.ClientDidReceiveData += ClientDidReceiveData;
            client.ClientDidSendData += ClientDidSendData;
            client.ClientDidNotSendData += ClientDidNotSendData;

            client.Search();
            textBox1.Enabled = false;
            button1.Enabled = false;
            button3.Enabled = false;
        }

        private void ClientDidStartSearching(Client client) 
        {
            //Console.WriteLine("CLIENT: Started Searching");
        }

        private void ClientDidNotSearch(Client client, Exception exception)
        {
            //Console.WriteLine("CLIENT: Failed to search. Reason: " + exception.ToString());
        }
        
        private void ClientDidUpdateServices(Client client)
        {
            listBox1.Items.Clear();
            foreach (NetService service in client.Services)
            {
                //Console.WriteLine(service.Name);
                listBox1.Items.Add(service.Name);
            }
        }

        private void ClientDidStartConnecting(Client client)
        {
            //Console.WriteLine("CLIENT: Connecting to server");
        }

        private void ClientDidConnect(Client client)
        {
            //Console.WriteLine("CLIENT: Connected to server");
            client.SendString("Hi server", Encoding.ASCII);
            textBox1.Enabled = true;
            button1.Enabled = true;
            button3.Enabled = true;
        }

        private void ClientDidNotConnect(Client client, Exception exception)
        {
            //Console.WriteLine("CLIENT: Did not connect to server. Reason: " + exception.ToString());
        }

        private void ClientDidDisconnect(Client client)
        {
            //Console.WriteLine("CLIENT: Disconnected");
        }
        
        private void ClientDidReceiveData(Client client, CommunicationData data)
        {
            Console.WriteLine("CLIENT: received data: " + Encoding.ASCII.GetString(data.DataContent.GetBytes()));
        }

        private void ClientDidSendData(Client client, CommunicationData data)
        {
            //Console.WriteLine("CLIENT: Sent data");
        }

        private void ClientDidNotSendData(Client client, CommunicationData data, Exception exception)
        {
            //Console.WriteLine("CLIENT: Failed to send: " + Encoding.ASCII.GetString(data) + "; Reason: " + exception.ToString());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0 && listBox1.SelectedIndex < client.Services.Count && (client.ConnectedState == ClientConnectedState.NotConnected || client.ConnectedState == ClientConnectedState.ErrorConnecting || client.ConnectedState == ClientConnectedState.Disconnected)) 
            {
                NetService service = client.Services[listBox1.SelectedIndex];
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
            client.SendString(textBox1.Text);
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
                    client.SendFile(filePath);
                }
            }  
        }
    }
}
