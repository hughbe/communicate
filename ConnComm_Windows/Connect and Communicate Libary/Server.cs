using System;
using System.Net;
using System.Net.Sockets;
using ZeroconfService;

namespace ConnComm
{
    #region Top Level Delegates

    public delegate void ServerDidPublish(Server server);
    public delegate void ServerDidNotPublish(Server server, Exception exception);
    public delegate void ServerDidUnpublish(Server server);
    public delegate void ServerDidStartListening(Server server);
    public delegate void ServerDidNotStartListening(Server server, Exception exception);
    public delegate void ServerDidStopListening(Server server);
    public delegate void ServerDidStartConnecting(Server server);
    public delegate void ServerDidConnect(Server server);
    public delegate void ServerDidNotConnect(Server server, Exception exception);
    public delegate void ServerDidDisconnect(Server server);
    public delegate void ServerDidReceiveData(Server server, byte[] data, int numberOfBytesTransferred);

    #endregion

    /// <summary>
    /// This class will publish itself on the network and await a connection request to it. 
    /// Once it receives a connection request, it will connect and will pass on data to the delegate as soon as data are received.
    /// </summary>
    public class Server : IDisposable
    {

        #region Private Variables

        private ServerInfo _serverInfo;
        private ProtocolInfo _protocolInfo;

        private NetService _publishedService;
        private Socket _listeningSocket;
        private Socket _connectedSocket;

        private bool _listening;

        private bool _publishing;
        private bool _connecting;

        private bool _published;
        private bool _connected;

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
        /// The backend information about the server that dictates how the server will listen for and handle incoming connection requests and streams of data
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
        /// The backend socket that handles sending and receiving data to the device the server is connected to
        /// </summary>
        public Socket ConnectedSocket
        {
            get { return _connectedSocket; }
        }

        /// <summary>
        /// A value that indicates whether the server is currently listening for incoming connection requests from other devices on the network
        /// </summary>
        public bool Listening
        {
            get { return _listening; }
        }

        /// <summary>
        /// A value that indicates whether the server is currently in the process of connecting to a device on the network
        /// </summary>
        public bool Connecting
        {
            get { return _connecting; }
        }

        /// <summary>
        /// A value that indicates whether the server is currently in the process of publishing itself on the network
        /// </summary>
        public bool Publishing
        {
            get { return _publishing; }
        }
        
        /// <summary>
        /// A value that indicates whether the server is currently connected to a device on the network
        /// </summary>
        public bool Connected
        {
            get { return _connected; }
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
        public ServerDidUnpublish ServerDidUnpublish;

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
        public ServerDidStartConnecting ServerDidStartConnecting;

        /// <summary>
        /// The delegate method called when the server successfully connects to a device on the network
        /// </summary>
        /// <param name="server">The server that successfully connected</param>
        public ServerDidConnect ServerDidConnect;

        /// <summary>
        /// The delegate method called when the server fails to connect to a device on the network
        /// </summary>
        /// <param name="server">The server that failed to connect</param>
        /// <param name="exception">The reason why the server failed to connect</param>
        public ServerDidNotConnect ServerDidNotConnect;

        /// <summary>
        /// The delegate method called when the server disconnects from a device on the network
        /// </summary>
        /// <param name="server">The server that disconnected from a device on the network</param>
        public ServerDidDisconnect ServerDidDisconnect;

        /// <summary>
        /// The delegate method called when the server receives data from a device it is connected to
        /// </summary>
        /// <param name="server">The server that received the data</param>
        /// <param name="data">The byte array of the data received. It can be encoded into a string or image etc.</param>
        /// <param name="numberOfBytesTransferred">The number of bytes transferred from client to server</param>
        public ServerDidReceiveData ServerDidReceiveData;

        #endregion

        #region Starting

        /// <summary>
        /// The default constructor for the Server object
        /// </summary>
        /// <param name="serverInfo">The backend information about the server that dictates how the server will listen for and handle incoming connection requests and streams of data</param>
        /// <param name="protocolInfo">The backend information about the server that dictates how the server will publish itself on the network</param>
        public Server(ServerInfo serverInfo, ProtocolInfo protocolInfo)
        {
            _serverInfo = serverInfo;
            _protocolInfo = protocolInfo;
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
        public void Publish() {
            _publishing = true;

            try
            {
                _publishedService = new NetService(_protocolInfo.Domain, _protocolInfo.Serialize(), _serverInfo.ReadableName, _serverInfo.Port);

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
        /// The delegate method called when the server successfully accepts a connection request from a device on the network and opens a socket to send and receive data streams between the connected devices
        /// </summary>
        /// <param name="sender">The socket that connected to the device on the network</param>
        /// <param name="e">The information about the connection of the server to a device on the network</param>
        private void AcceptCallback(object sender, SocketAsyncEventArgs e)
        {
            _connecting = true;
            if (ServerDidStartConnecting != null)
            {
                ServerDidStartConnecting(this);
            }

            Socket listenSocket = (Socket)sender;
            try
            {
                _connectedSocket = e.AcceptSocket;
                SocketAsyncEventArgs socketAsynEventArgs = new SocketAsyncEventArgs();
                socketAsynEventArgs.Completed += SocketDidReceiveData;
                socketAsynEventArgs.SetBuffer(new byte[ServerInfo.DataBufferSize], 0, ServerInfo.DataBufferSize);
                _connectedSocket.ReceiveAsync(socketAsynEventArgs);

                _connecting = false;
                _connected = true;
                if (ServerDidConnect != null)
                {
                    ServerDidConnect(this);
                }
            }
            catch (Exception exception)
            {
                _connecting = false;
                _connected = false;
                if (ServerDidNotConnect != null)
                {
                    ServerDidNotConnect(this, exception);
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

        /// <summary>
        /// The delegate method called when the server receives data sent from a device on the network that it is connected to
        /// </summary>
        /// <param name="sender">The socket that received the data from a connected device on the network</param>
        /// <param name="e"></param>
        private void SocketDidReceiveData(object sender, SocketAsyncEventArgs e)
        {
            if (!_connectedSocket.Connected)
            {
                if (ServerDidNotConnect == null)
                {
                    _connected = false;
                    ServerDidDisconnect(this);
                }
                return;
            }

            byte[] data = e.Buffer;
            if (ServerDidReceiveData != null)
            {
                ServerDidReceiveData(this, data, e.BytesTransferred);
            }
         
            _connectedSocket.ReceiveAsync(e);
        }
                
        #endregion

        #region Ending

        /// <summary>
        /// Unpublishes the server from the network
        /// </summary>
        public void Unpublish()
        {
            _published = false;
            if (_publishedService != null)
            {
                _publishedService.Stop();
                _publishedService = null;
                if (ServerDidUnpublish != null)
                {
                    ServerDidUnpublish(this);
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
            _connected = false;
            if (_connectedSocket != null)
            {
                _connectedSocket.Close();
                _connectedSocket = null;
                if (ServerDidDisconnect != null)
                {
                    ServerDidDisconnect(this);
                }
            }
        }

        /// <summary>
        /// Stops the server by unpublishing it from the network, stopping listening for incoming connection requests and disconnecting from any devices on the network that the server is currently connected to
        /// </summary>
        public void Stop()
        {
            Unpublish();
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
                if (_connectedSocket != null) { _connectedSocket.Dispose(); _connectedSocket = null; }
                if (_listeningSocket != null) { _listeningSocket.Dispose(); _listeningSocket = null; }
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
            return "Server: connected = " + _connected.ToString() + "; published = " + _published.ToString() + "; " + _serverInfo.ToString() + "; " + _protocolInfo.ToString();
        }
    }
}
