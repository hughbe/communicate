using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConnComm;

namespace Demo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Server server;

        private void Form1_Load(object sender, EventArgs e)
        {
            TXTRecordList recordList = new TXTRecordList();
            recordList.AddTXTRecord("platform", "windows");
            recordList.AddTXTRecord("publish_time", DateTime.Now.ToString());

            int dataBufferSize = 256;

            ServerInfo serverInfo = new ServerInfo(Environment.MachineName, 12345, recordList, dataBufferSize);
            ProtocolInfo protocolInfo = new ProtocolInfo("_ClickBoard", TransportProtcolType.TCP, ProtocolInfo.ProtocolDomainLocal);

            server = new Server(serverInfo, protocolInfo);

            server.ServerDidPublish += ServerDidPublish;
            server.ServerDidNotPublish += ServerDidNotPublish;
            server.ServerDidUnpublish += ServerDidUnpublish;

            server.ServerDidStartListening += ServerDidStartListening;
            server.ServerDidNotStartListening += ServerDidNotStartListening;
            server.ServerDidStopListening += ServerDidStopListening;

            server.ServerDidStartConnecting += ServerDidStartConnecting;
            server.ServerDidConnect += ServerDidConnect;

            server.ServerDidNotConnect += ServerDidNotConnect;
            server.ServerDidDisconnect += ServerDidDisconnect;

            server.ServerDidReceiveData += ServerDidReceiveData;

            server.Publish();
            server.StartListening();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Stop();
        }

        private void ServerDidPublish(Server server)
        {
            Console.WriteLine("Published server");
        }

        private void ServerDidNotPublish(Server server, Exception exception)
        {
            Console.WriteLine("Did not publish server. Reason: " + exception.ToString());
        }

        private void ServerDidUnpublish(Server server)
        {
            Console.WriteLine("Unpublished server");
        }

        private void ServerDidStartListening(Server server)
        {
            Console.WriteLine("Started listening");
        }

        private void ServerDidNotStartListening(Server server, Exception exception)
        {
            Console.WriteLine("Did not start listening. Reason: " + exception.ToString());
        }

        private void ServerDidStopListening(Server server)
        {
            Console.WriteLine("Stopped listening");
        }

        private void ServerDidStartConnecting(Server server)
        {
            Console.WriteLine("Starting to connect");
        }

        private void ServerDidConnect(Server server)
        {
            Console.WriteLine("Connected");
        }

        private void ServerDidNotConnect(Server server, Exception exception)
        {
            Console.WriteLine("Did not connect. Reason: " + exception.ToString());
        }

        private void ServerDidDisconnect(Server server)
        {
            Console.WriteLine("Server disconnected");
        }

        private void ServerDidReceiveData(Server server, byte[] data, int numberOfBytesTransferred)
        {
            string dataInStringForm = Encoding.ASCII.GetString(data, 0, numberOfBytesTransferred);
            Console.WriteLine(dataInStringForm);
        }
    }
}
