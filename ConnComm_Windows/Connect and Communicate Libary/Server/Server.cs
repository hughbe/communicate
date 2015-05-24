using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using ZeroconfService;
using System.Text;
using Communicate.Common;
using System.Drawing;
using System.IO;
using System.Collections.ObjectModel;

namespace Communicate.Server
{
    #region Top Level Delegates

    public delegate void ServerDidPublish(Server server);
    public delegate void ServerDidNotPublish(Server server, Exception exception);
    public delegate void ServerDidUnPublish(Server server);
    public delegate void ServerDidStartListening(Server server);
    public delegate void ServerDidNotStartListening(Server server, Exception exception);
    public delegate void ServerDidStopListening(Server server);
    public delegate void ServerDidStartConnectingToClient(Server server, ConnectedClient client);
    public delegate void ServerDidConnectToClient(Server server, ConnectedClient client);
    public delegate void ServerDidNotConnectToClient(Server server, ConnectedClient client, Exception exception);
    public delegate void ServerDidDisconnectFromClient(Server server, ConnectedClient client);
    public delegate void ServerDidReceiveDataFromClient(Server server, ConnectedClient client, byte[] data, string footerContents, DataType dataType);
    public delegate void ServerDidSendDataToClient(Server server, byte[] data, ConnectedClient client);
    public delegate void ServerDidNotSendDataToClient(Server server, byte[] data, Exception exception, ConnectedClient client);

    #endregion

    /// <summary>
    /// This class will publish itself on the network and await a connection request to it. 
    /// Once it receives a connection request, it will connect and will pass on contentData to the delegate as soon as contentData are received.
    /// </summary>
    public class Server : IDisposable
    {

        #region Private Variables

        private ServerInfo _serverInfo;
        private ProtocolInfo _protocolInfo;

        private NetService _publishedService;
        private Socket _listeningSocket;

        private ConnectedClientsCollection _connectedClients;

        private bool _listening;

        private bool _publishing;
        private bool _published;

        #endregion

        #region Properties

        /// <summary>
        /// The service currently published on the network. This may be null
        /// </summary>
        public NetService PublishedService
        {
            get { return _publishedService; }
        }

        /// <summary>
        /// The backend information about the server that dictates how the server will listen for and handle incoming connection requests and streams of contentData
        /// </summary>
        public ServerInfo ServerInfo
        {
            get { return _serverInfo; }
        }

        /// <summary>
        /// The backend information about the server that dictates how the server will publish itself on the network
        /// </summary>
        public ProtocolInfo ProtocolInfo
        {
            get { return _protocolInfo; }
        }

        /// <summary>
        /// The backend socket that handles and detects incoming connection requests
        /// </summary>
        public Socket ListeningSocket
        {
            get { return _listeningSocket; }
        }

        /// <summary>
        /// The list of clients the server is currently connected to
        /// </summary>
        public ConnectedClientsCollection ConnectedClients
        {
            get { return _connectedClients; }
        }

        /// <summary>
        /// A value that indicates whether the server is currently listening for incoming connection requests from other devices on the network
        /// </summary>
        public bool Listening
        {
            get { return _listening; }
        }
        
        /// <summary>
        /// A value that indicates whether the server is currently in the process of publishing itself on the network
        /// </summary>
        public bool Publishing
        {
            get { return _publishing; }
        }
        
        /// <summary>
        /// A value that indicates whether the server has published itself on the network
        /// </summary>
        public bool Published
        {
            get { return _published; }
        }

        #endregion 

        #region Delegates

        /// <summary>
        /// The delegate method called when the server publishes a service on the network successfully
        /// </summary>
        /// <param name="server">The server that published the service</param>
        public ServerDidPublish ServerDidPublish;

        /// <summary>
        /// The delegate method called when the server fails to publishe a service on the network
        /// </summary>
        /// <param name="server">The server that failed to publish the service</param>
        /// <param name="exception">The reason why the service failed to publish</param>
        public ServerDidNotPublish ServerDidNotPublish;

        /// <summary>
        /// The delegate method called when the server unpublishes the service from the network
        /// </summary>
        /// <param name="server">The server that unpublished the service</param>
        public ServerDidUnPublish ServerDidUnPublish;

        /// <summary>
        /// The delegate method called when the server starts listening for a connection request to it
        /// </summary>
        /// <param name="server">The server listening for a connection request</param>
        public ServerDidStartListening ServerDidStartListening;

        /// <summary>
        /// The delegate method called when the server fails to start listening for a connection request to it
        /// </summary>
        /// <param name="server">The server that failed to start listening for a connection request</param>
        /// <param name="exception">The reason why the server failed to start listening for a connection request</param>
        public ServerDidNotStartListening ServerDidNotStartListening;

        /// <summary>
        /// The delegate method called when the server stops listening for a connection request to it
        /// </summary>
        /// <param name="server">The server that stopped listening for a connection request</param>
        public ServerDidStopListening ServerDidStopListening;

        /// <summary>
        /// The delegate method called when the server starts to initiating a connection with an incoming connection request
        /// </summary>
        /// <param name="server">The server that is attempting to initiate the connection</param>
        /// <param name="client">The client to which did start connecting</param>
        public ServerDidStartConnectingToClient ServerDidStartConnectingToClient;

        /// <summary>
        /// The delegate method called when the server successfully connects to a device on the network
        /// </summary>
        /// <param name="server">The server that successfully connected</param>
        /// <param name="client">The client to which the server connected</param>
        public ServerDidConnectToClient ServerDidConnectToClient;

        /// <summary>
        /// The delegate method called when the server fails to connect to a device on the network
        /// </summary>
        /// <param name="server">The server that failed to connect</param>
        /// <param name="client">The client to which the server failed to connect</param>
        /// <param name="exception">The reason why the server failed to connect</param>
        public ServerDidNotConnectToClient ServerDidNotConnectToClient;

        /// <summary>
        /// The delegate method called when the server disconnects from a device on the network
        /// </summary>
        /// <param name="server">The server that disconnected from a device on the network</param>
        /// <param name="client">The client that the server disconnected from</param>
        public ServerDidDisconnectFromClient ServerDidDisconnectFromClient;

        /// <summary>
        /// The delegate method called when the server receives contentData from a device it is connected to
        /// </summary>
        /// <param name="server">The server that received the contentData</param>
        /// <param name="contentData">The byte array of the contentData received. It can be encoded into a string or image etc.</param>
        /// <param name="numberOfBytesTransferred">The number of bytes transferred from client to server</param>
        /// <param name="client">The client that sent the contentData</param>
        public ServerDidReceiveDataFromClient ServerDidReceiveDataFromClient;

        /// <summary>
        /// The delegate method called when the server sends contentData to a connected client
        /// </summary>
        /// <param name="contentData">The byte array of the contentData sent. It can be encoded into a string or image etc.</param>
        /// <param name="client">The client to which the contentData was sent</param>
        public ServerDidSendDataToClient ServerDidSendDataToClient;

        /// <summary>
        /// The delegate method called when the server fails to send contentData to a connected client
        /// </summary>
        /// <param name="contentData">The byte array of the contentData that failed to be sent. It can be encoded into a string or image etc.</param>
        /// <param name="exception">The reason for the failure to send the contentData</param>
        /// <param name="client">The client to which sending contentData failed</param>
        public ServerDidNotSendDataToClient ServerDidNotSendDataToClient;

        #endregion

        #region Starting

        /// <summary>
        /// The default constructor for the Server object
        /// </summary>
        /// <param name="serverInfo">The backend information about the server that dictates how the server will listen for and handle incoming connection requests and streams of contentData</param>
        /// <param name="protocolInfo">The backend information about the server that dictates how the server will publish itself on the network</param>
        public Server(ServerInfo serverInfo, ProtocolInfo protocolInfo)
        {
            _serverInfo = serverInfo;
            _protocolInfo = protocolInfo;
            _connectedClients = new ConnectedClientsCollection();
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private Server()
        {

        }
        
        /// <summary>
        /// Publishes the server on the network and starts listening for any incoming connection requests from other devices on the network
        /// </summary>
        public void PublishAndListen()
        {
            Publish();
            StartListening();
        }

        /// <summary>
        /// Publishes the server on the network using the information given using the ProtocolInfo and ServerInfo objects used to construct the Server object
        /// </summary>
        public void Publish()
        {
            _publishing = true;

            try
            {
                _publishedService = new NetService(_protocolInfo.Domain, _protocolInfo.SerializeType(), _serverInfo.ReadableName, _serverInfo.Port);

                _publishedService.TXTRecordData = NetService.DataFromTXTRecordDictionary(_serverInfo.TXTRecordList.Serialize());

                _publishedService.DidPublishService += new NetService.ServicePublished(DidPublishService);
                _publishedService.DidNotPublishService += new NetService.ServiceNotPublished(DidNotPublishService);

                _publishedService.Publish();
            }
            catch (Exception exception)
            {
                _publishing = false;
                if (ServerDidNotPublish != null)
                {
                    ServerDidNotPublish(this, exception);
                }
            }
        }

        /// <summary>
        /// Opens a socket using the information given using the ProtocolInfo and ServerInfo objects used to construct the Server object to listen and handle any incoming connection requests
        /// </summary>
        public void StartListening()
        {
            _listening = true;
            try
            {
                _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listeningSocket.Bind(new IPEndPoint(IPAddress.Any, _serverInfo.Port));
                _listeningSocket.Listen(10);
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.Completed += AcceptCallback;
                _listeningSocket.AcceptAsync(e);
                if (ServerDidStartListening != null)
                {
                    ServerDidStartListening(this);
                }
            }
            catch (Exception ex)
            {
                _listening = false;
                if (ServerDidNotStartListening != null)
                {
                    ServerDidNotStartListening(this, ex);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the NetService successfully publishes itself on the network
        /// </summary>
        /// <param name="service">The service that was published on the network</param>
        private void DidPublishService(NetService service)
        {
            _publishing = false;
            _published = true;
            _publishedService = service;
            if (ServerDidPublish != null)
            {
                ServerDidPublish(this);
            }
        }

        /// <summary>
        /// The delegate method called when the NetService fails to publish itself on the network
        /// </summary>
        /// <param name="service">The service that failed to be published on the network</param>
        /// <param name="exception">The reason why the service failed to publish</param>
        private void DidNotPublishService(NetService service, DNSServiceException exception)
        {
            _publishing = false;
            _published = false;
            if (ServerDidNotPublish != null)
            {
                ServerDidNotPublish(this, exception);
            }
        }

        /// <summary>
        /// The delegate method called when the server successfully accepts a connection request from a device on the network and opens a socket to send and receive contentData streams between the connected devices
        /// </summary>
        /// <param name="sender">The socket that connected to the device on the network</param>
        /// <param name="e">The information about the connection of the server to a device on the network</param>
        private void AcceptCallback(object sender, SocketAsyncEventArgs e)
        {
            ConnectedClient client = new ConnectedClient(this, e.AcceptSocket);
            client.State = ClientState.Connecting;
            if (ServerDidStartConnectingToClient != null)
            {
                ServerDidStartConnectingToClient(this, client);
            }

            Socket listenSocket = (Socket)sender;
            try
            {
                client.StartReceiving();

                client.State = ClientState.Connected;
                this.ConnectedClients.AddClient(client);
                if (ServerDidConnectToClient != null)
                {
                    ServerDidConnectToClient(this, client);
                }
            }
            catch (Exception exception)
            {
                client.State = ClientState.NotConnected;
                if (ServerDidNotConnectToClient != null)
                {
                    ServerDidNotConnectToClient(this, client, exception);
                }
            }
            finally
            {
                e.AcceptSocket = null; // to enable reuse
            }
            try
            {
                listenSocket.AcceptAsync(e);
            }
            catch { }
        }
                
        #endregion

        #region Sending and Receiving

        #region To All Clients

        /// <summary>
        /// Sends a string to all clients
        /// </summary>
        /// <param name="stringToSend">The string to send using ASCII encoding</param>
        public void SendStringToAllClients(string stringToSend)
        {
            SendStringToAllClients(stringToSend, Encoding.ASCII);
        }

        /// <summary>
        /// Sends a string of a particular encoding to all clients
        /// </summary>
        /// <param name="stringToSend">The string to send using ASCII encoding</param>
        /// <param name="encoding">The encoding of the string to send to the client</param>
        public void SendStringToAllClients(string stringToSend, Encoding encoding)
        {
            SendDataToAllClients(DataSerializer.StringToByteArray(stringToSend, encoding));
        }

        /// <summary>
        /// Sends an image to all clients
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="name">The name of the image to send</param>
        public void SendImageToAllClients(Image image, string name)
        {
            SendDataToAllClients(DataSerializer.ImageToByteArray(image, name));
        }

        /// <summary>
        /// Sends a file to all clients
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        public void SendFileToAllClients(string filePath)
        {
            SendDataToAllClients(File.ReadAllBytes(filePath));
        }
        
        /// <summary>
        /// Sends contentData to all clients
        /// </summary>
        /// <param name="dataToSend">The contentData to send to the client</param>
        public void SendDataToAllClients(byte[] dataToSend) 
        {
            SendDataToClient(dataToSend, null);
        }

        /// <summary>
        /// Sends contentData to all clients
        /// </summary>
        /// <param name="dataList">The contentData list to send to the client</param>
        public void SendDataToAllClients(Collection<byte[]> dataList)
        {
            SendDataToClient(dataList, null);
        }

        #endregion

        /// <summary>
        /// Sends a string to a client
        /// </summary>
        /// <param name="client">The client to send the string</param>
        /// <param name="stringToSend">The string to send to the client using ASCII encoding</param>
        public void SendStringToClient(string stringToSend, ConnectedClient client)
        {
            SendDataToClient(DataSerializer.StringToByteArray(stringToSend, Encoding.ASCII), client);
        }

        /// <summary>
        /// Sends a string of a particular encoding to a client
        /// </summary>
        /// <param name="stringToSend">The string to send to the client</param>
        /// <param name="encoding">The encoding of the string to send to the client</param>
        /// <param name="client">The client to send the string to</param>
        public void SendStringToClient(string stringToSend, Encoding encoding, ConnectedClient client)
        {
            SendDataToClient(DataSerializer.StringToByteArray(stringToSend, encoding), client);
        }

        /// <summary>
        /// Sends an image to a client
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="name">The name of the image to send</param>
        /// <param name="client">The client to send the image to</param>
        public void SendImageToClient(Image image, string name, ConnectedClient client)
        {
            SendDataToClient(DataSerializer.ImageToByteArray(image, name), client);
        }

        /// <summary>
        /// Sends a file to a client
        /// </summary>
        /// <param name="filePath">The path of the file to send to the server</param>
        /// <param name="client">The client to send the file to</param>
        public void SendFileToClient(string filePath, ConnectedClient client)
        {
            SendDataToClient(File.ReadAllBytes(filePath), client);
        }

        /// <summary>
        /// Sends contentData to a client
        /// </summary>
        /// <param name="dataToSend">The contentData to send to the client</param>
        /// <param name="client">The client to send the contentData to</param>
        public void SendDataToClient(byte[] dataToSend, ConnectedClient client)
        {
            Collection<byte[]> dataList = DataSerializer.DataToByteArray(dataToSend);
            if (client != null)
            {
                client.SendData(dataList);
            }
            else
            {
                foreach(ConnectedClient connectedClient in _connectedClients) {
                    connectedClient.SendData(dataList);
                }
            }
        }

        private void SendDataToClient(Collection<byte[]> dataList, ConnectedClient client)
        {
            if (dataList.Count >= 3)
            {
                if (client != null)
                {
                    client.SendData(dataList);
                }
                else
                {
                    foreach (ConnectedClient connectedClient in _connectedClients)
                    {
                        connectedClient.SendData(dataList);
                    }
                }
            }
        }

        #endregion

        #region Ending

        /// <summary>
        /// UnPublishes the server from the network
        /// </summary>
        public void UnPublish()
        {
            _published = false;
            if (_publishedService != null)
            {
                _publishedService.Stop();
                _publishedService = null;
                if (ServerDidUnPublish != null)
                {
                    ServerDidUnPublish(this);
                }
            }
        }

        /// <summary>
        /// Stops the server from listening and handling incoming connection requests
        /// </summary>
        public void StopListening()
        {
            _listening = false;
            if (_listeningSocket != null)
            {
                _listeningSocket.Close();
                _listeningSocket = null;
                if (ServerDidStopListening != null)
                {
                    ServerDidStopListening(this);
                }
            }
        }

        /// <summary>
        /// Disconnects the server from the device on the network it is connected to
        /// </summary>
        public void Disconnect()
        {
            ConnectedClients.DisconnectAll();
        }

        /// <summary>
        /// Stops the server by unpublishing it from the network, stopping listening for incoming connection requests and disconnecting from any devices on the network that the server is currently connected to
        /// </summary>
        public void Stop()
        {
            UnPublish();
            StopListening();
            Disconnect();
        }

        /// <summary>
        /// Dipsposes all objects managed by the server
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects managed by the server
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connectedClients != null) { _connectedClients.DisconnectAll(); _connectedClients.Dispose(); _connectedClients = null; }
                if (_listeningSocket != null) { _listeningSocket.Close(); _listeningSocket = null; }
                if (_publishedService != null) { _publishedService.Dispose(); _publishedService = null; }
            }
        }

        #endregion

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the server in a readable format</returns>
        public override string ToString()
        {
            return "Server: connected services = " + _connectedClients.ToString() + "; published = " + _published.ToString() + "; " + _serverInfo.ToString() + "; " + _protocolInfo.ToString();
        }
    }
}
