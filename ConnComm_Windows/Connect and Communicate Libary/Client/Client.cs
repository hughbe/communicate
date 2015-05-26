using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using ZeroconfService;
using System.Net;
using System.Net.Sockets;
using Communicate.Common;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Collections.ObjectModel;
using System.Web.Script.Serialization;

namespace Communicate.Client
{
    #region Top Level Delegates

    public delegate void ClientDidStartSearching(Client client);
    public delegate void ClientDidNotSearch(Client client, Exception exception);
    public delegate void ClientDidUpdateServices(Client client);
    public delegate void ClientDidStartConnecting(Client client);
    public delegate void ClientDidNotConnect(Client client, Exception exception);
    public delegate void ClientDidConnect(Client client);
    public delegate void ClientDidDisconnect(Client client);
    public delegate void ClientDidReceiveData(Client client, CommunicationData data);
    public delegate void ClientDidSendData(Client client, CommunicationData data);
    public delegate void ClientDidNotSendData(Client client, CommunicationData data, Exception exception);

    #endregion
    
    public class Client : IDisposable
    {
        #region Private Variables

        private ClientDidStartSearching _clientDidStartSearching;
        private ClientDidNotSearch _clientDidNotSearch;
        private ClientDidUpdateServices _clientDidUpdateServices;

        private ClientDidStartConnecting _clientDidStartConnecting;
        private ClientDidNotConnect _clientDidNotConnect;
        private ClientDidConnect _clientDidConnect;
        private ClientDidDisconnect _clientDidDisconnect;

        private ClientDidReceiveData _clientDidReceiveData;
        private ClientDidSendData _clientDidSendData;
        private ClientDidNotSendData _clientDidNotSendData;

        private ProtocolInfo _protocolInfo;
        private ClientInfo _clientInfo;

        private NetServiceBrowser _browser;
        private Collection<NetService> _services;

        private NetService _connectingService;

        private NetService _connectedService;
        private ConnectionHandler _connectionHandler;

        private ClientSearchingState _searchingState;
        private ClientConnectedState _connectedState;

        #endregion

        #region Properties

        /// <summary>
        /// The delegate method called when the client starts searching for a server
        /// </summary>
        /// <param name="client">The client that started searching</param>
        public ClientDidStartSearching ClientDidStartSearching
        {
            get { return _clientDidStartSearching; }
            set { _clientDidStartSearching = value; }
        }

        /// <summary>
        /// The delegate method called when the client fails to find for a server
        /// </summary>
        /// <param name="client">The client that failed to find a server</param>
        /// <param name="exception">The reason for the failure to find a server</param>
        public ClientDidNotSearch ClientDidNotSearch
        {
            get { return _clientDidNotSearch; }
            set { _clientDidNotSearch = value; }
        }

        /// <summary>
        /// The delegate method called when the client finds or loses a list of servers
        /// </summary>
        /// <param name="client">The client that found or lost a list of servers</param>
        public ClientDidUpdateServices ClientDidUpdateServices
        {
            get { return _clientDidUpdateServices; }
            set { _clientDidUpdateServices = value; }
        }

        /// <summary>
        /// The delegate method called when the client starts to connect to a server
        /// </summary>
        /// <param name="client">The client that started to connect to a server</param>
        public ClientDidStartConnecting ClientDidStartConnecting
        {
            get { return _clientDidStartConnecting; }
            set { _clientDidStartConnecting = value; }
        }

        /// <summary>
        /// The delegate method called when the client fails to connect to a server
        /// </summary>
        /// <param name="client">The client that failed to connect to a server</param>
        /// <param name="exception">The reason for the failure to connect to a server</param>
        public ClientDidNotConnect ClientDidNotConnect
        {
            get { return _clientDidNotConnect; }
            set { _clientDidNotConnect = value; }
        }

        /// <summary>
        /// The delegate method called when the client successfully connects to a server
        /// </summary>
        /// <param name="client">The client that connnected to the server</param>
        public ClientDidConnect ClientDidConnect
        {
            get { return _clientDidConnect; }
            set { _clientDidConnect = value; }
        }

        /// <summary>
        /// The delegate method called when the client disconnects from a server
        /// </summary>
        /// <param name="client">The client that disconnnected to the server</param>
        public ClientDidDisconnect ClientDidDisconnect
        {
            get { return _clientDidDisconnect; }
            set { _clientDidDisconnect = value; }
        }

        /// <summary>
        /// The delegate method called when the client receives data from a device it is connected to
        /// </summary>
        /// <param name="client">The client that received the data</param>
        /// <param name="data">The data received. It can be encoded into a string or image etc.</param>
        public ClientDidReceiveData ClientDidReceiveData
        {
            get { return _clientDidReceiveData; }
            set { _clientDidReceiveData = value; }
        }

        /// <summary>
        /// The delegate method called when the client sends data to a connected server
        /// </summary>
        /// <param name="client">The client that sent the contentData</param>
        /// <param name="data">The data sent. It can be encoded into a string or image etc.</param>
        public ClientDidSendData ClientDidSendData
        {
            get { return _clientDidSendData; }
            set { _clientDidSendData = value; }
        }

        /// <summary>
        /// The delegate method called when the client fails to send data to a connected server
        /// </summary>
        /// <param name="client">The client that failed to send the contentData</param>
        /// <param name="data">The data that failed to send. It can be encoded into a string or image etc.</param>
        /// <param name="exception">The reason for the failure to send the data</param>
        public ClientDidNotSendData ClientDidNotSendData
        {
            get { return _clientDidNotSendData; }
            set { _clientDidNotSendData = value; }
        }

        /// <summary>
        /// The backend information about the client that dictates how the client will search for devices on the network
        /// </summary>
        public ProtocolInfo ProtocolInfo
        {
            get { return _protocolInfo; }
        }

        /// <summary>
        /// The backend information about the client that dictates how the client will open and handle connection to a device on the network
        /// </summary>
        public ClientInfo ClientInfo
        {
            get { return _clientInfo;  }
        }

        /// <summary>
        /// The browser that searches for devices on the network. This may be null
        /// </summary>
        public NetServiceBrowser Browser
        {
            get { return _browser; }
        }

        /// <summary>
        /// The list of services that have been found on the network. This may be null
        /// </summary>
        public Collection<NetService> Services 
        {
            get { return _services; }
        }

        /// <summary>
        /// A value that indicates the searching state of the client
        /// </summary>
        public ClientSearchingState SearchingState
        {
            get { return _searchingState; }
        }

        /// <summary>
        /// A value that indicates the connected state of the client
        /// </summary>
        public ClientConnectedState ConnectedState
        {
            get { return _connectedState; }
        }

        /// <summary>
        /// The NetService the client is currently connecting to. This may be null
        /// </summary>
        public NetService ConnectingService
        {
            get { return _connectingService; }
        }

        /// <summary>
        /// The NetService the client is currently connected to. This may be null
        /// </summary>
        public NetService ConnectedService
        {
            get { return _connectedService; }
        }

        /// <summary>
        /// The connection handler for the client
        /// </summary>
        internal ConnectionHandler ConnectionHandler
        {
            get { return _connectionHandler; }
        }

        #endregion

        #region Starting

        /// <summary>
        /// The default constructor for the Client object
        /// </summary>
        /// <param name="protocolInfo">The backend information about the client that dictates how the client will search for devices on the network</param>
        /// <param name="clientInfo">The backend information about the client that dictates how the client will open and handle connection to a device on the network</param>
        public Client(ProtocolInfo protocolInfo, ClientInfo clientInfo)
        {
            _protocolInfo = protocolInfo;
            _clientInfo = clientInfo;
            _searchingState = ClientSearchingState.NotSearching;
            _connectedState = ClientConnectedState.NotConnected;

            _connectionHandler = new ConnectionHandler(_clientInfo);
            _connectionHandler.ConnectionHandlerDidConnect = ConnectionHandlerDidConnect;
            _connectionHandler.ConnectionHandlerDidDisconnect = ConnectionHandlerDidDisconnect;
            _connectionHandler.ConnectionHandlerDidReceiveData = ConnectionHandlerDidReceiveData;
            _connectionHandler.ConnectionHandlerDidSendData = ConnectionHandlerDidSendData;
            _connectionHandler.ConnectionHandlerDidNotSendData = ConnectionHandlerDidNotSendData;
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private Client()
        {

        }

        /// <summary>
        /// Searches for devices on the network published that comply with the ProtocolInfo of this Client
        /// </summary>
        public void Search()
        {
            StopSearching();
            _searchingState = ClientSearchingState.Searching;
            _services = new Collection<NetService>();
            try
            {
                _browser = new NetServiceBrowser();
                _browser.DidFindService += DidFindService;
                _browser.DidRemoveService += DidRemoveService;
                _browser.SearchForService(_protocolInfo.SerializeType(false), _protocolInfo.Domain);
            }
            catch (Exception exception)
            {
                _searchingState = ClientSearchingState.ErrorSearching;
                if (_clientDidNotSearch != null)
                {
                    _clientDidNotSearch(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the NetServiceBrowser finds a service
        /// </summary>
        /// <param name="browser">The NetServiceBrowser that found the service</param>
        /// <param name="service">The NetService that the NetServiceBrowser found</param>
        /// <param name="moreComing">Whether more services are to be found</param>
        private void DidFindService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            bool same = false;
            foreach (NetService aService in _services) 
            {
                if (aService.Name.Equals(service.Name))
                {
                    same = true;
                }
            }
            if (!same)
            {
                _services.Add(service);
            }
            if (!moreComing)
            {
                if (_clientDidUpdateServices != null)
                {
                    _clientDidUpdateServices(this);
                }
            }
        }

        /// <summary>
        /// The delegate method called when a NetService is no longer available
        /// </summary>
        /// <param name="browser">The NetServiceBrowser that originally found the service</param>
        /// <param name="service">The NetService that is no longer connected</param>
        /// <param name="moreComing">Whether more services are to be removed</param>
        private void DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            _services.Remove(service);
            if (!moreComing)
            {
                if (_clientDidUpdateServices != null)
                {
                    _clientDidUpdateServices(this);
                }
            }
        }

        #endregion

        #region Connecting

        /// <summary>
        /// Start the process to connect the client to a computer on the network
        /// </summary>
        /// <param name="service">The computer, represented by a NetService object, to connect to</param>
        public void ConnectToService(NetService service)
        {
            if(_connectedState == ClientConnectedState.Connecting || service == null) 
            {
                return;
            }
            _connectedState = ClientConnectedState.Connecting;
            _connectingService = service;
            if (_clientDidStartConnecting != null)
            {
                _clientDidStartConnecting(this);
            }
            try
            {
                service.DidResolveService += DidResolveService;
                service.DidNotResolveService += DidNotResolveService;
                service.ResolveWithTimeout(10);
            }
            catch (Exception exception)
            {
                _connectedState = ClientConnectedState.ErrorConnecting;
                if (_clientDidNotConnect != null)
                {
                    _clientDidNotConnect(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the client resolves a device on the network
        /// </summary>
        /// <param name="service">The service that the client connected to</param>
        private void DidResolveService(NetService service)
        {
            if (_connectedState == ClientConnectedState.Connected)
            {
                return;
            }
            try
            {
                TcpClient client = _connectionHandler.TCPClient;
                if (service.Addresses.Count > 0)
                {
                    IPEndPoint endPoint = (IPEndPoint)service.Addresses[0];
                    client.Connect(endPoint);
                }
                else
                {
                    client.Connect(service.HostName, service.Port);
                }
                EndConnecting();

                if (client.Connected && _connectedState != ClientConnectedState.Connected)
                {
                    _connectedService = service;
                    _connectionHandler.ConnectToSocket(client.Client);
                }
                else if (!client.Connected)
                {
                    EndConnecting();
                    _connectedState = ClientConnectedState.ErrorConnecting;
                    if (_clientDidNotConnect != null)
                    {
                        _clientDidNotConnect(this, new Exception("Error opening socket"));
                    }
                }
            }
            catch (Exception exception)
            {
                EndConnecting();
                Disconnect();
                if (_clientDidNotConnect != null)
                {
                    _clientDidNotConnect(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the client fails to connect to a device on the network
        /// </summary>
        /// <param name="service">The service that the client failed to connect to</param>
        /// <param name="exception">The reason why the client failed to connect</param>
        private void DidNotResolveService(NetService service, DNSServiceException exception)
        {
            _connectedState = ClientConnectedState.ErrorConnecting;
            if (_clientDidNotConnect != null)
            {
                _clientDidNotConnect(this, exception);
            }
        }
        /// <summary>
        /// The delegate method called when the connection handler connects
        /// </summary>
        /// <param name="connectionHandler">The connection handler that connected</param>
        private void ConnectionHandlerDidConnect(ConnectionHandler connectionHandler)
        {
            _connectedState = ClientConnectedState.Connected;
            if (_clientDidConnect != null)
            {
                _clientDidConnect(this);
            }
        }

        /// <summary>
        /// The delegate method called when the connection handler disconnects
        /// </summary>
        /// <param name="connectionHandler">The connection handler that disconnected</param>
        private void ConnectionHandlerDidDisconnect(ConnectionHandler connectionHandler)
        {
            _connectedState = ClientConnectedState.Disconnected;
            if (_clientDidDisconnect != null)
            {
                _clientDidDisconnect(this);
            }
        }

        /// <summary>
        /// The delegate method called when the connection handler receives data
        /// </summary>
        /// <param name="connectionHandler">The connection handler that received the data</param>
        /// <param name="data">The data received</param>
        private void ConnectionHandlerDidReceiveData(ConnectionHandler connectionHandler, CommunicationData data)
        {
            if (_clientDidReceiveData != null)
            {
                _clientDidReceiveData(this, data);
            }
        }

        /// <summary>
        /// The delegate method called when the connection handler sends data successfully
        /// </summary>
        /// <param name="connectionHandler">The connection handler that sent the data</param>
        /// <param name="data">The data sent</param>
        private void ConnectionHandlerDidSendData(ConnectionHandler connectionHandler, CommunicationData data)
        {
            if (_clientDidSendData != null)
            {
                _clientDidSendData(this, data);
            }
        }

        /// <summary>
        /// The delegate method called when the connection handler fails to send data successfully
        /// </summary>
        /// <param name="connectionHandler">The connection handler that failed to send the data</param>
        /// <param name="data">The data failed to send</param>
        /// <param name="reason">The reason why the data failed to send</param>
        private void ConnectionHandlerDidNotSendData(ConnectionHandler connectionHandler, CommunicationData data, Exception reason)
        {
            if (_clientDidNotSendData != null)
            {
                _clientDidNotSendData(this, data, reason);
            }
        }

        #endregion

        #region Sending and Receiving
        
        /// <summary>
        /// Sends a string to the server
        /// </summary>
        /// <param name="stringToSend">The string to send to the server using ASCII encoding</param>
        public void SendString(string stringToSend)
        {
            SendString(stringToSend, Encoding.ASCII);
        }

        /// <summary>
        /// Sends a string of a particular encoding to the server
        /// </summary>
        /// <param name="stringToSend">The string to send to the server</param>
        /// <param name="encoding">The encoding of the string to send to the server</param>
        public void SendString(string stringToSend, Encoding encoding)
        {
            _connectionHandler.SendString(stringToSend, encoding);
        }

        /// <summary>
        /// Sends an untitled image to the server
        /// </summary>
        /// <param name="image">The image to send to the server</param>
        public void SendImage(Image image)
        {
            SendImage(image, "Untitled");
        }

        /// <summary>
        /// Sends a image to the server
        /// </summary>
        /// <param name="image">The image to send to the server</param>
        /// <param name="name">The name of the image to send</param>
        public void SendImage(Image image, string name)
        {
            _connectionHandler.SendImage(image, name);
        }

        /// <summary>
        /// Sends a file to the server
        /// </summary>
        /// <param name="filePath">The path of the file to send to the server</param>
        public void SendFile(string filePath)
        {
            SendFile(filePath, Path.GetFileName(filePath));
        }
        
        /// <summary>
        /// Sends a file to the server
        /// </summary>
        /// <param name="filePath">The path of the file to send to the server</param>
        /// <param name="name">The name of the file to send to the server</param>
        public void SendFile(string filePath, string name)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            if (DataDetector.IsValidImage(bytes))
            {
                _connectionHandler.SendImage(Image.FromFile(filePath), name);
            }
            else
            {
                _connectionHandler.SendFile(filePath, name);
            }
        }

        /// <summary>
        /// Sends a dictionary encoded in JSON to the server
        /// </summary>
        /// <param name="dictionary">The dictionary to send</param>
        public void SendDictionary(Dictionary<object, object> dictionary) 
        {
            _connectionHandler.SendDictionary(dictionary);
        }

        /// <summary>
        /// Sends an array encoded in JSON to the server
        /// </summary>
        /// <param name="array">The array to send</param>
        public void SendArray(List<object> array)
        {
            _connectionHandler.SendArray(array);
        }

        /// <summary>
        /// Sends a JSON string to the server
        /// </summary>
        /// <param name="JSONString">The JSON string to send</param>
        public void SendJSONString(string JSONString) 
        {
            _connectionHandler.SendJSONString(JSONString);
        }
        
        /// <summary>
        /// Sends data to the server
        /// </summary>
        /// <param name="dataToSend">The data to send to the server</param>
        public void SendData(byte[] dataToSend)
        {
            _connectionHandler.SendData(dataToSend);
        }

        /// <summary>
        /// Sends data to the server
        /// </summary>
        /// <param name="data">The communication data to send to the server</param>
        public void SendData(CommunicationData data)
        {
            _connectionHandler.SendData(data);
        }

        #endregion

        #region Ending

        /// <summary>
        /// Stops the client from searching for devices on the network
        /// </summary>
        public void StopSearching()
        {
            _searchingState = ClientSearchingState.StoppedSearching;
            if (_browser != null)
            {
                _browser.Stop();
                _browser.Dispose();
                _browser = null;
            }
        }

        /// <summary>
        /// Ends the connecting process. Could signify that a successful connection has been made, or that the connection process has fail
        /// </summary>
        private void EndConnecting() {
            if(_connectingService != null) {
                _connectingService.Stop();
                _connectingService.Dispose();
                _connectingService = null;
            }
        }

        /// <summary>
        /// Disconnects the client from a device on the network
        /// </summary>
        public void Disconnect()
        {
            _connectedState = ClientConnectedState.Disconnected;
            EndConnecting();
            if (_connectionHandler != null)
            {
                _connectionHandler.Disconnect();
                _connectionHandler = null;
            }
        }

        /// <summary>
        /// Stops the client by stopping searching for devices on the network, stops connecting to a device on the network and disconnects from any devices on the network
        /// </summary>
        public void Stop()
        {
            StopSearching();
            Disconnect();
        }
        /// <summary>
        /// Dipsposes all objects managed by the client
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects managed by the client
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_browser != null) { _browser.Stop(); _browser.Dispose(); _browser = null; }
                if (_connectingService != null) { _connectingService.Stop(); _connectingService.Dispose(); _connectingService = null; }
                if (_connectedService != null) { _connectedService.Stop(); _connectedService.Dispose(); _connectedService = null; }
                if (_connectionHandler != null) { _connectionHandler.Disconnect(); _connectionHandler = null; }
            }
        }

        #endregion
    }
}
