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

    public delegate void ServerDidStartPublishing(Server server);
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
        
    public delegate void ServerDidReceiveDataFromClient(Server server, ConnectedClient client, CommunicationData data);
    public delegate void ServerDidSendDataToClient(Server server, CommunicationData data, ConnectedClient client);
    public delegate void ServerDidNotSendDataToClient(Server server, CommunicationData data, Exception exception, ConnectedClient client);

    #endregion

    /// <summary>
    /// This class will publish itself on the network and await a connection request to it. 
    /// Once it receives a connection request, it will connect and will pass on contentData to the delegate as soon as contentData are received.
    /// </summary>
    public class Server : IDisposable
    {

        #region Private Variables

        private ServerDidStartPublishing _serverDidStartPublishing;
        private ServerDidPublish _serverDidPublish;
        private ServerDidNotPublish _serverDidNotPublish;
        private ServerDidUnPublish _serverDidUnPublish;

        private ServerDidStartListening _serverDidStartListening;
        private ServerDidNotStartListening _serverDidNotStartListening;
        private ServerDidStopListening _serverDidStopListening;

        private ServerDidStartConnectingToClient _serverDidStartConnectingToClient;
        private ServerDidConnectToClient _serverDidConnectToClient;
        private ServerDidNotConnectToClient _serverDidNotConnectToClient;
        private ServerDidDisconnectFromClient _serverDidDisconnectFromClient;

        private ServerDidReceiveDataFromClient _serverDidReceiveDataFromClient;
        private ServerDidSendDataToClient _serverDidSendDataToClient;
        private ServerDidNotSendDataToClient _serverDidNotSendDataToClient;

        private ServerInfo _serverInfo;
        private ProtocolInfo _protocolInfo;

        private NetService _publishedService;
        private Socket _listeningSocket;

        private ConnectedClientsCollection _connectedClients;

        private ServerPublishingState _publishingState;
        private ServerListeningState _listeningState;

        #endregion

        #region Properties

        /// <summary>
        /// The delegate method called when the server starts to publish a service on the network
        /// </summary>
        /// <param name="server">The server that started to publish the service</param>
        public ServerDidStartPublishing ServerDidStartPublishing
        {
            get { return _serverDidStartPublishing; }
            set { _serverDidStartPublishing = value; }
        }

        /// <summary>
        /// The delegate method called when the server publishes a service on the network successfully
        /// </summary>
        /// <param name="server">The server that published the service</param>
        public ServerDidPublish ServerDidPublish
        {
            get { return _serverDidPublish; }
            set { _serverDidPublish = value; }
        }

        /// <summary>
        /// The delegate method called when the server fails to publishe a service on the network
        /// </summary>
        /// <param name="server">The server that failed to publish the service</param>
        /// <param name="exception">The reason why the service failed to publish</param>
        public ServerDidNotPublish ServerDidNotPublish
        {
            get { return _serverDidNotPublish; }
            set { _serverDidNotPublish = value; }
        }

        /// <summary>
        /// The delegate method called when the server unpublishes the service from the network
        /// </summary>
        /// <param name="server">The server that unpublished the service</param>
        public ServerDidUnPublish ServerDidUnPublish
        {
            get { return _serverDidUnPublish; }
            set { _serverDidUnPublish = value; }
        }

        /// <summary>
        /// The delegate method called when the server starts listening for a connection request to it
        /// </summary>
        /// <param name="server">The server listening for a connection request</param>
        public ServerDidStartListening ServerDidStartListening
        {
            get { return _serverDidStartListening; }
            set { _serverDidStartListening = value; }
        }

        /// <summary>
        /// The delegate method called when the server fails to start listening for a connection request to it
        /// </summary>
        /// <param name="server">The server that failed to start listening for a connection request</param>
        /// <param name="exception">The reason why the server failed to start listening for a connection request</param>
        public ServerDidNotStartListening ServerDidNotStartListening
        {
            get { return _serverDidNotStartListening; }
            set { _serverDidNotStartListening = value; }
        }

        /// <summary>
        /// The delegate method called when the server stops listening for a connection request to it
        /// </summary>
        /// <param name="server">The server that stopped listening for a connection request</param>
        public ServerDidStopListening ServerDidStopListening
        {
            get { return _serverDidStopListening; }
            set { _serverDidStopListening = value; }
        }

        /// <summary>
        /// The delegate method called when the server starts to initiating a connection with an incoming connection request
        /// </summary>
        /// <param name="server">The server that is attempting to initiate the connection</param>
        /// <param name="client">The client to which did start connecting</param>
        public ServerDidStartConnectingToClient ServerDidStartConnectingToClient
        {
            get { return _serverDidStartConnectingToClient; }
            set { _serverDidStartConnectingToClient = value; }
        }

        /// <summary>
        /// The delegate method called when the server successfully connects to a device on the network
        /// </summary>
        /// <param name="server">The server that successfully connected</param>
        /// <param name="client">The client to which the server connected</param>
        public ServerDidConnectToClient ServerDidConnectToClient
        {
            get { return _serverDidConnectToClient; }
            set { _serverDidConnectToClient = value; }
        }

        /// <summary>
        /// The delegate method called when the server fails to connect to a device on the network
        /// </summary>
        /// <param name="server">The server that failed to connect</param>
        /// <param name="client">The client to which the server failed to connect</param>
        /// <param name="exception">The reason why the server failed to connect</param>
        public ServerDidNotConnectToClient ServerDidNotConnectToClient
        {
            get { return _serverDidNotConnectToClient; }
            set { _serverDidNotConnectToClient = value; }
        }

        /// <summary>
        /// The delegate method called when the server disconnects from a device on the network
        /// </summary>
        /// <param name="server">The server that disconnected from a device on the network</param>
        /// <param name="client">The client that the server disconnected from</param>
        public ServerDidDisconnectFromClient ServerDidDisconnectFromClient
        {
            get { return _serverDidDisconnectFromClient; }
            set { _serverDidDisconnectFromClient = value; }
        }

        /// <summary>
        /// The delegate method called when the server receives contentData from a device it is connected to
        /// </summary>
        /// <param name="server">The server that received the contentData</param>
        /// <param name="contentData">The byte array of the contentData received. It can be encoded into a string or image etc.</param>
        /// <param name="numberOfBytesTransferred">The number of bytes transferred from client to server</param>
        /// <param name="client">The client that sent the contentData</param>
        public ServerDidReceiveDataFromClient ServerDidReceiveDataFromClient
        {
            get { return _serverDidReceiveDataFromClient; }
            set { _serverDidReceiveDataFromClient = value; }
        }

        /// <summary>
        /// The delegate method called when the server sends contentData to a connected client
        /// </summary>
        /// <param name="contentData">The byte array of the contentData sent. It can be encoded into a string or image etc.</param>
        /// <param name="client">The client to which the contentData was sent</param>
        public ServerDidSendDataToClient ServerDidSendDataToClient
        {
            get { return _serverDidSendDataToClient; }
            set { _serverDidSendDataToClient = value; }
        }

        /// <summary>
        /// The delegate method called when the server fails to send contentData to a connected client
        /// </summary>
        /// <param name="contentData">The byte array of the contentData that failed to be sent. It can be encoded into a string or image etc.</param>
        /// <param name="exception">The reason for the failure to send the contentData</param>
        /// <param name="client">The client to which sending contentData failed</param>
        public ServerDidNotSendDataToClient ServerDidNotSendDataToClient
        {
            get { return _serverDidNotSendDataToClient; }
            set { _serverDidNotSendDataToClient = value; }
        }

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
        /// A value that indicates the server's publishing state
        /// </summary>
        public ServerPublishingState PublishingState
        {
            get { return _publishingState; }
        }
        
        /// <summary>
        /// A value that indicates the server's listening state
        /// </summary>
        public ServerListeningState ListeningState
        {
            get { return _listeningState; }
        }
        

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

            _publishingState = ServerPublishingState.NotPublished;
            _listeningState = ServerListeningState.NotListening;
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
            _publishingState = ServerPublishingState.Publishing;
            try
            {
                _publishedService = new NetService(_protocolInfo.Domain, _protocolInfo.SerializeType(false), _serverInfo.ReadableName, _serverInfo.Port);

                _publishedService.TXTRecordData = _serverInfo.TXTRecordList.Serialize();

                _publishedService.DidPublishService += new NetService.ServicePublished(DidPublishService);
                _publishedService.DidNotPublishService += new NetService.ServiceNotPublished(DidNotPublishService);

                _publishedService.Publish();
                if (_serverDidStartPublishing != null)
                {
                    _serverDidStartPublishing(this);
                }
            }
            catch (Exception exception)
            {
                _publishingState = ServerPublishingState.ErrorPublishing;
                if (_serverDidNotPublish != null)
                {
                    _serverDidNotPublish(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the NetService successfully publishes itself on the network
        /// </summary>
        /// <param name="service">The service that was published on the network</param>
        private void DidPublishService(NetService service)
        {
            _publishingState = ServerPublishingState.Published;
            _publishedService = service;
            if (_serverDidPublish != null)
            {
                _serverDidPublish(this);
            }
        }

        /// <summary>
        /// The delegate method called when the NetService fails to publish itself on the network
        /// </summary>
        /// <param name="service">The service that failed to be published on the network</param>
        /// <param name="exception">The reason why the service failed to publish</param>
        private void DidNotPublishService(NetService service, DNSServiceException exception)
        {
            _publishingState = ServerPublishingState.ErrorPublishing;
            if (_serverDidNotPublish != null)
            {
                _serverDidNotPublish(this, exception);
            }
        }

        /// <summary>
        /// Opens a socket using the information given using the ProtocolInfo and ServerInfo objects used to construct the Server object to listen and handle any incoming connection requests
        /// </summary>
        public void StartListening()
        {
            _listeningState = ServerListeningState.Listening;
            try
            {
                TcpListener listener = new TcpListener(IPAddress.Any, _serverInfo.Port);
                listener.Start(10);
                listener.BeginAcceptSocket(new AsyncCallback(ListenerAcceptSocketCallback), listener);
                if (_serverDidStartListening != null)
                {
                    _serverDidStartListening(this);
                }
            }
            catch (Exception ex)
            {
                _listeningState = ServerListeningState.ErrorListening;
                if (_serverDidNotStartListening != null)
                {
                    _serverDidNotStartListening(this, ex);
                }
            }
        }

        // Process the client connection. 
        public void ListenerAcceptSocketCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            Socket clientSocket = listener.EndAcceptSocket(ar);

            // Process the connection here. (Add the client to a  
            // server table, read data, etc.)
            Console.WriteLine("Client connected completed");

            ConnectedClient client = new ConnectedClient(this, clientSocket);
            // Signal the calling thread to continue.
            if (_serverDidStartConnectingToClient != null)
            {
                _serverDidStartConnectingToClient(this, client);
            }

            try
            {
                client.State = ClientState.Connected;
                this.ConnectedClients.AddClient(client);
            }
            catch (Exception exception)
            {
                client.State = ClientState.NotConnected;
                if (_serverDidNotConnectToClient != null)
                {
                    _serverDidNotConnectToClient(this, client, exception);
                }
            }

            listener.BeginAcceptSocket(new AsyncCallback(ListenerAcceptSocketCallback), listener);
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
            SendStringToClient(stringToSend, encoding, null);
        }

        /// <summary>
        /// Sends an untitled image to all clients
        /// </summary>
        /// <param name="image">The image to send</param>
        public void SendImageToClient(Image image)
        {
            SendImageToAllClients(image, "Untitled");
        }

        /// <summary>
        /// Sends an image to all clients
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="name">The name of the image to send</param>
        public void SendImageToAllClients(Image image, string name)
        {
            SendImageToClient(image, name, null);
        }

        /// <summary>
        /// Sends a file to all clients
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        public void SendFileToAllClients(string filePath)
        {
            SendFileToAllClients(filePath, Path.GetFileName(filePath));
        }

        /// <summary>
        /// Sends a file to all clients
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        /// <param name="name">The name of the file to send</param>
        public void SendFileToAllClients(string filePath, string name)
        {
            SendFileToClient(filePath, name, null);
        }
        /// <summary>
        /// Sends a dictionary encoded in JSON to all clients
        /// </summary>
        /// <param name="dictionary">The dictionary to send</param>
        public void SendDictionaryToAllClients(Dictionary<object, object> dictionary)
        {
            SendDictionary(dictionary, null);
        }

        /// <summary>
        /// Sends an array encoded in JSON to all clients
        /// </summary>
        /// <param name="array">The array to send</param>
        public void SendArrayToAllClients(List<object> array)
        {
            SendArray(array, null);
        }

        /// <summary>
        /// Sends a JSON string to all clients
        /// </summary>
        /// <param name="JSONString">The JSON string to send</param>
        public void SendJSONString(string JSONString)
        {
            SendJSONString(JSONString, null);
        }
        
        /// <summary>
        /// Sends data to all clients
        /// </summary>
        /// <param name="dataToSend">The data to send</param>
        public void SendDataToAllClients(byte[] dataToSend) 
        {
            SendDataToClient(dataToSend, null);
        }

        /// <summary>
        /// Sends communication to all clients
        /// </summary>
        /// <param name="data">The communication data to send</param>
        public void SendDataToAllClients(CommunicationData data)
        {
            SendDataToClient(data, null);
        }

        #endregion

        /// <summary>
        /// Sends a string to a client
        /// </summary>
        /// <param name="client">The client to send the string</param>
        /// <param name="stringToSend">The string to send to the client using ASCII encoding</param>
        public void SendStringToClient(string stringToSend, ConnectedClient client)
        {
            SendStringToClient(stringToSend, Encoding.ASCII, client);
        }

        /// <summary>
        /// Sends a string of a particular encoding to a client
        /// </summary>
        /// <param name="stringToSend">The string to send to the client</param>
        /// <param name="encoding">The encoding of the string to send to the client</param>
        /// <param name="client">The client to send the string to</param>
        public void SendStringToClient(string stringToSend, Encoding encoding, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendString(stringToSend, encoding);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendString(stringToSend, encoding);
                }
            }
        }

        /// <summary>
        /// Sends an untitled image to a client
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="client">The client to send the image to</param>
        public void SendImageToClient(Image image, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendImage(image, "Untitled");
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendImage(image, "Untitled");
                }
            }
        }

        /// <summary>
        /// Sends an image to a client
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="name">The name of the image to send</param>
        /// <param name="client">The client to send the image to</param>
        public void SendImageToClient(Image image, string name, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendImage(image, name);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendImage(image, name);
                }
            }
        }

        /// <summary>
        /// Sends a file to a client
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        /// <param name="client">The client to send the file to</param>
        public void SendFileToClient(string filePath, ConnectedClient client)
        {
            SendFileToClient(filePath, client);
        }

        /// <summary>
        /// Sends a file to a client
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        /// <param name="client">The client to send the file to</param>
        public void SendFileToClient(string filePath, string name, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendFile(filePath, name);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendFile(filePath, name);
                }
            }
        }

        /// <summary>
        /// Sends a dictionary encoded in JSON to a client
        /// </summary>
        /// <param name="dictionary">The dictionary to send</param>
        /// <param name="client">The client to send the dictionary to</param>
        public void SendDictionary(Dictionary<object, object> dictionary, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendDictionary(dictionary);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendDictionary(dictionary);
                }
            }
        }

        /// <summary>
        /// Sends an array encoded in JSON to a client
        /// </summary>
        /// <param name="array">The array to send</param>
        /// <param name="client">The client to send the array to</param>
        public void SendArray(List<object> array, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendArray(array);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendArray(array);
                }
            }
        }

        /// <summary>
        /// Sends a JSON string to a client
        /// </summary>
        /// <param name="JSONString">The JSON string to send</param>
        /// <param name="client">The client to send the JSON string to</param>
        public void SendJSONString(string JSONString, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendJSONString(JSONString);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendJSONString(JSONString);
                }
            }
        }

        /// <summary>
        /// Sends data to a client
        /// </summary>
        /// <param name="dataToSend">The data to send to the client</param>
        /// <param name="client">The client to send the data to</param>
        public void SendDataToClient(byte[] dataToSend, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendData(dataToSend);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendData(dataToSend);
                }
            }
        }

        /// <summary>
        /// Sends data to a client
        /// </summary>
        /// <param name="data">The data to send to the client</param>
        /// <param name="client">The client to send the data to</param>
        private void SendDataToClient(CommunicationData data, ConnectedClient client)
        {
            if (client != null)
            {
                client.ConnectionHandler.SendData(data);
            }
            else
            {
                foreach (ConnectedClient connectedClient in _connectedClients)
                {
                    connectedClient.ConnectionHandler.SendData(data);
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
            _listeningState = ServerListeningState.NotListening;
            if (_publishedService != null)
            {
                _publishedService.Stop();
                _publishedService = null;
                if (_serverDidUnPublish != null)
                {
                    _serverDidUnPublish(this);
                }
            }
        }

        /// <summary>
        /// Stops the server from listening and handling incoming connection requests
        /// </summary>
        public void StopListening()
        {
            _listeningState = ServerListeningState.StoppedListening;
            if (_listeningSocket != null)
            {
                _listeningSocket.Close();
                _listeningSocket = null;
                if (_serverDidStopListening != null)
                {
                    _serverDidStopListening(this);
                }
            }
        }

        /// <summary>
        /// Disconnects the server from the device on the network it is connected to
        /// </summary>
        public void Disconnect()
        {
            ConnectedClients.DisconnectAllClients();
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
                if (_connectedClients != null) { _connectedClients.DisconnectAllClients(); _connectedClients.Dispose(); _connectedClients = null; }
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
            return "Server: connected services = " + _connectedClients.ToString() + "; published = " + _publishingState.ToString() + "; listening = " + _listeningState.ToString() + "; " + _serverInfo.ToString() + "; " + _protocolInfo.ToString();
        }
    }
}
