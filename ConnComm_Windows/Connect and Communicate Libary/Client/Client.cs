using System;
using System.Collections.Generic;
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
    public delegate void ClientDidReceiveData(Client client, byte[] data, string footerContent, DataType dataType);
    public delegate void ClientDidSendData(Client client, byte[] data);
    public delegate void ClientDidNotSendData(Client client, byte[] data, Exception exception);

    #endregion

    public class Client : IDisposable
    {
        #region Private Variables

        private ProtocolInfo _protocolInfo;
        private ClientInfo _clientInfo;

        private NetServiceBrowser _browser;
        private Collection<NetService> _services;

        private NetService _connectingService;

        private NetService _connectedService;
        private Socket _connectedSocket;

        private bool _searching;
        private bool _connecting;
        private bool _connected;

        #endregion

        #region Properties

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
        /// A value indicating whether the client is currently searching for devices on the network
        /// </summary>
        public bool Searching
        {
            get { return _searching; }
        }

        /// <summary>
        /// A value indicating whether the client is currently in the process of connecting to a device on the network
        /// </summary>
        public bool Connecting
        {
            get { return _connecting; }
        }

        /// <summary>
        /// A value indicating whether the client is currently connected to a device on the network
        /// </summary>
        public bool Connected
        {
            get { return _connected; }
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
        /// The socket the client is current connected to. This may be null
        /// </summary>
        public Socket ConnectedSocket
        {
            get { return _connectedSocket; }
        }

        #endregion

        #region Delegates

        /// <summary>
        /// The delegate method called when the client starts searching for a server
        /// </summary>
        /// <param name="client">The client that started searching</param>
        public ClientDidStartSearching ClientDidStartSearching;

        /// <summary>
        /// The delegate method called when the client fails to find for a server
        /// </summary>
        /// <param name="client">The client that failed to find a server</param>
        /// <param name="exception">The reason for the failure to find a server</param>
        public ClientDidNotSearch ClientDidNotSearch;

        /// <summary>
        /// The delegate method called when the client finds or loses a list of servers
        /// </summary>
        /// <param name="client">The client that found or lost a list of servers</param>
        public ClientDidUpdateServices ClientDidUpdateServices;

        /// <summary>
        /// The delegate method called when the client starts to connect to a server
        /// </summary>
        /// <param name="client">The client that started to connect to a server</param>
        public ClientDidStartConnecting ClientDidStartConnecting;

        /// <summary>
        /// The delegate method called when the client fails to connect to a server
        /// </summary>
        /// <param name="client">The client that failed to connect to a server</param>
        /// <param name="exception">The reason for the failure to connect to a server</param>
        public ClientDidNotConnect ClientDidNotConnect;

        /// <summary>
        /// The delegate method called when the client successfully connects to a server
        /// </summary>
        /// <param name="client">The client that connnected to the server</param>
        public ClientDidConnect ClientDidConnect;

        /// <summary>
        /// The delegate method called when the client disconnects from a server
        /// </summary>
        /// <param name="client">The client that disconnnected to the server</param>
        public ClientDidDisconnect ClientDidDisconnect;

        /// <summary>
        /// The delegate method called when the client receives contentData from a device it is connected to
        /// </summary>
        /// <param name="client">The client that received the contentData</param>
        /// <param name="contentData">The byte array of the contentData received. It can be encoded into a string or image etc.</param>
        /// <param name="numberOfBytesTransferred">The number of bytes transferred from server to client</param>
        public ClientDidReceiveData ClientDidReceiveData;

        /// <summary>
        /// The delegate method called when the client sends contentData to a connected server
        /// </summary>
        /// <param name="client">The client that sent the contentData</param>
        /// <param name="contentData">The byte array of the contentData sent. It can be encoded into a string or image etc.</param>
        public ClientDidSendData ClientDidSendData;

        /// <summary>
        /// The delegate method called when the client fails to send contentData to a connected server
        /// </summary>
        /// <param name="client">The client that failed to send the contentData</param>
        /// <param name="contentData">The byte array of the contentData that failed to be sent. It can be encoded into a string or image etc.</param>
        /// <param name="exception">The reason for the failure to send the contentData</param>
        public ClientDidNotSendData ClientDidNotSendData;

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
        }

        /// <summary>
        /// Searches for devices on the network published that comply with the ProtocolInfo of this Client
        /// </summary>
        public void Search()
        {
            StopSearching();
            _searching = true;
            _services = new Collection<NetService>();
            try
            {
                _browser = new NetServiceBrowser();
                _browser.DidFindService += DidFindService;
                _browser.DidRemoveService += DidRemoveService;
                _browser.SearchForService(_protocolInfo.SerializeType(), _protocolInfo.Domain);
            }
            catch (Exception exception)
            {
                _searching = false;
                if (ClientDidNotSearch != null)
                {
                    ClientDidNotSearch(this, exception);
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
                if (ClientDidUpdateServices != null)
                {
                    ClientDidUpdateServices(this);
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
                if (ClientDidUpdateServices != null)
                {
                    ClientDidUpdateServices(this);
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
            if(_connecting || service == null) 
            {
                return;
            }
            _connecting = true;
            _connectingService = service;
            if (ClientDidStartConnecting != null)
            {
                ClientDidStartConnecting(this);
            }
            try
            {
                service.DidResolveService += DidResolveService;
                service.DidNotResolveService += DidNotResolveService;
                service.ResolveWithTimeout(10);
            }
            catch (Exception exception)
            {
                StopConnecting();
                if (ClientDidNotConnect != null)
                {
                    ClientDidNotConnect(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the client resolves a device on the network
        /// </summary>
        /// <param name="service">The service that the client connected to</param>
        private void DidResolveService(NetService service)
        {
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                    socket.Connect(service.HostName, _clientInfo.Port);
                    _connecting = false;
                    EndConnecting();
                    if (socket.Connected && !_connected)
                    {
                        _connected = true;
                        _connectedService = service;
                        _connectedSocket = socket;

                        Thread backgroundThread = new Thread(new ThreadStart(Receive));
                        backgroundThread.IsBackground = true;
                        backgroundThread.Start();

                        if (ClientDidConnect != null)
                        {
                            ClientDidConnect(this);
                        }

                    }
                    else if (!socket.Connected)
                    {
                        EndConnecting();
                        Disconnect();
                        if (ClientDidNotConnect != null)
                        {
                            ClientDidNotConnect(this, new Exception("Error opening socket"));
                        }
                    }
            }
            catch (Exception exception)
            {
                EndConnecting();
                Disconnect();
                if (ClientDidNotConnect != null)
                {
                    ClientDidNotConnect(this, exception);
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
            StopConnecting();
            if (ClientDidNotConnect != null)
            {
                ClientDidNotConnect(this, exception);
            }
        }

        #endregion

        #region Sending and Receiving

        /// <summary>
        /// The synchronous method for receiving contentData
        /// </summary>
        private void Receive()
        {
            try
            {
                while (true)
                {

                    byte[] headerData = new byte[10];
                    int headerBytesRead = 0;
                    while (headerBytesRead < headerData.Length)
                    {
                        int read = _connectedSocket.Receive(headerData, headerBytesRead, headerData.Length - headerBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            headerBytesRead += read;
                        }
                    }

                    byte[] typeBuffer = new byte[2] { headerData[0], headerData[1] };
                    DataType receivingDataType = DataSerializer.DataTypeFromByteArray(typeBuffer);

                    int contentLength = BitConverter.ToInt32(headerData, 2);
                    int footerLength = BitConverter.ToInt32(headerData, 6);

                    byte[] contentData = new byte[contentLength];
                    int contentBytesRead = 0;
                    while (contentBytesRead < contentData.Length)
                    {
                        int read = _connectedSocket.Receive(contentData, contentBytesRead, contentData.Length - contentBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            contentBytesRead += read;
                        }
                    }

                    byte[] footerData = new byte[footerLength];
                    int footerBytesRead = 0;
                    while (footerBytesRead < footerData.Length)
                    {
                        int read = _connectedSocket.Receive(footerData, footerBytesRead, footerData.Length - footerBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            footerBytesRead += read;
                        }
                    }
                    string footerContent = "";
                    if (footerData.Length > 0)
                    {
                        footerContent = DataSerializer.ByteArrayToString(footerData);
                    }

                    if (ClientDidReceiveData != null)
                    {
                        ClientDidReceiveData(this, contentData, footerContent, receivingDataType);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

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
            SendData(DataSerializer.StringToByteArray(stringToSend, encoding));
        }

        /// <summary>
        /// Sends a image to the server
        /// </summary>
        /// <param name="image">The image to send to the server</param>
        /// <param name="name">The name of the image to send</param>
        public void SendImage(Image image, string name)
        {
            SendData(DataSerializer.ImageToByteArray(image, name));
        }

        /// <summary>
        /// Sends a file to the server
        /// </summary>
        /// <param name="filePath">The path of the file to send to the server</param>
        public void SendFile(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            if (DataDetector.IsValidImage(bytes))
            {
                SendImage(Image.FromFile(filePath), Path.GetFileName(filePath));
            }
            else
            {
                SendData(DataSerializer.FileToByteArray(filePath));
            }
        }

        /// <summary>
        /// Sends contentData to the server
        /// </summary>
        /// <param name="dataList">The contentData to send to the server</param>
        public void SendData(Collection<byte[]> dataList)
        {
            if (dataList == null || dataList.Count < 3)
            {
                return;
            }
            byte[] header = dataList[0];
            byte[] bytes = dataList[1];
            byte[] footer = dataList[2];

            if (_connected && _connectedSocket != null)
            {
                try
                {
                    _connectedSocket.Send(header);
                    _connectedSocket.Send(bytes);
                    _connectedSocket.Send(footer);
                    if (ClientDidSendData != null)
                    {
                        ClientDidSendData(this, bytes);
                    }
                }
                catch (Exception exception)
                {
                    if (ClientDidNotSendData != null)
                    {
                        ClientDidNotSendData(this, bytes, exception);
                    }
                }
            }
        }

        /// <summary>
        /// Sends contentData to the server
        /// </summary>
        /// <param name="dataToSend">The contentData to send to the server</param>
        public void SendData(byte[] dataToSend)
        {
            SendData(DataSerializer.DataToByteArray(dataToSend));
        }

        #endregion

        #region Ending

        /// <summary>
        /// Stops the client from searching for devices on the network
        /// </summary>
        public void StopSearching()
        {
            _searching = false;
            if (_browser != null)
            {
                _browser.Stop();
                _browser.Dispose();
                _browser = null;
            }
        }
        
        /// <summary>
        /// Stops the client from connecting to a device on the network
        /// </summary>
        public void StopConnecting()
        {
            _connecting = false;
            if (_connectingService != null)
            {
                EndConnecting();
                if (ClientDidNotConnect != null)
                {
                    ClientDidNotConnect(this, new Exception("User ended the connecting process"));
                }
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
            _connected = false;
            if (_connectedService != null)
            {
                _connectedService.Stop();
                _connectedService.Dispose();
                _connectedService = null;
            }

            if (_connectedSocket != null)
            {
                _connectedSocket.Close();
                _connectedService = null;
                _connectedSocket = null;
                if (ClientDidDisconnect != null)
                {
                    ClientDidDisconnect(this);
                }
            }
        }

        /// <summary>
        /// Stops the client by stopping searching for devices on the network, stops connecting to a device on the network and disconnects from any devices on the network
        /// </summary>
        public void Stop()
        {
            StopSearching();
            StopConnecting();
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
                if (_connectedSocket != null) { _connectedSocket.Close(); _connectedSocket = null; }
            }
        }

        #endregion
    }
}
