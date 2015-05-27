using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Communicate.Common;
using ZeroconfService;
using System.Net;
using System.Web.Script.Serialization;
using System.Threading;
using System.Drawing;
using System.IO;

namespace Communicate.Connecting
{
    internal delegate void ConnectionStartedConnecting(Connection connection);
    internal delegate void ConnectionConnected(Connection connection);
    internal delegate void ConnectionNotConnected(Connection connection, Exception reason);
    internal delegate void ConnectionDisconnected(Connection connection);

    internal delegate void ConnectionReceivedData(Connection connection, CommunicationData data);
    internal delegate void ConnectionSentData(Connection connection, CommunicationData data);
    internal delegate void ConnectionNotSentData(Connection connection, CommunicationData data, Exception reason);

    public class Connection
    {
        #region Private Variables

        private ConnectedState _connectedState;

        private Socket _socket;
        private NetService _netService;

        private Thread _backgroundThread;
        #endregion

        #region Properties

        /// <summary>
        /// A value that indicates the connected state of a connection
        /// </summary>
        public ConnectedState ConnectedState
        {
            get { return _connectedState; }
        }

        /// <summary>
        /// The backend socket that handles connecting and sending and receiving data
        /// </summary>
        public Socket Socket
        {
            get { return _socket; }
        }
        /// <summary>
        /// The event called when the connection starts to connect to a device on the network
        /// </summary>
        internal event ConnectionStartedConnecting DidStartConnecting;

        /// <summary>
        /// The event called when the connection successfully connects to a device on the network
        /// </summary>
        internal event ConnectionConnected DidConnect;

        /// <summary>
        /// The event called when the connection fails to connect to a device on the network
        /// </summary>
        internal event ConnectionNotConnected DidNotConnect;

        /// <summary>
        /// The event called when the connection disconnects from a device on the network
        /// </summary>
        internal event ConnectionDisconnected DidDisconnect;

        /// <summary>
        /// The event called when the connection receives data from a device on the network
        /// </summary>
        internal event ConnectionReceivedData DidReceiveData;

        /// <summary>
        /// The event called when the connection sends data to a device on the network
        /// </summary>
        internal event ConnectionSentData DidSendData;

        /// <summary>
        /// The event called when the connection fails to send data to a device on the network
        /// </summary>
        internal event ConnectionNotSentData DidNotSendData;

        #endregion

        #region Starting

        /// <summary>
        /// Constructs a connection with a backend socekt
        /// </summary>
        /// <param name="socket">The backend socket that handles connecting and sending and receiving data</param>
        public Connection(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException("socket");
            }
            else
            {
                _connectedState = ConnectedState.NotConnected;
                _socket = socket;
            }
        }

        public Connection(NetService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
            else
            {
                _netService = service;
                _netService.DidResolveService += _netService_DidResolveService;
                _netService.DidNotResolveService += _netService_DidNotResolveService;
            }
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private Connection()
        {

        }

        /// <summary>
        /// Starts connecting to the device on the network the connection from which the connection was constructed
        /// </summary>
        public void Connect()
        {
            if ( _connectedState == ConnectedState.Connected)
            {
                return;
            }
            if (_connectedState != ConnectedState.Connecting)
            {
                _connectedState = ConnectedState.Connecting;
                if (DidStartConnecting != null)
                {
                    DidStartConnecting(this);
                }
            }

            if (_socket != null)
            {
                try
                {
                    _connectedState = ConnectedState.Connected;
                    _backgroundThread = new Thread(new ThreadStart(Receive));
                    _backgroundThread.IsBackground = true;
                    _backgroundThread.Start();
                    if (DidConnect != null)
                    {
                        DidConnect(this);
                    }
                }
                catch (Exception exception)
                {
                    _connectedState = ConnectedState.ErrorConnecting;
                    if (DidNotConnect != null)
                    {
                        DidNotConnect(this, exception);
                    }
                }
            }
            else if (_netService != null)
            {
                try
                {
                    _netService.ResolveWithTimeout(10);
                }
                catch (Exception exception)
                {
                    _connectedState = ConnectedState.ErrorConnecting;
                    if (DidNotConnect != null)
                    {
                        DidNotConnect(this, exception);
                    }
                }
            }
        }

        /// <summary>
        /// The delegate method called when the connection successfully resolves a Bonjour service
        /// </summary>
        /// <param name="service">The Bonjour service that was resolved</param>
        private void _netService_DidResolveService(NetService service)
        {
            if (_netService == null)
            {
                return;
            }
            _netService.Dispose();
            _netService = null;
            try
            {
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
                if (service.Addresses.Count > 0)
                {
                    IPEndPoint endPoint = (IPEndPoint)service.Addresses[0];
                    socket.Connect(endPoint as EndPoint);
                }
                else
                {
                    socket.Connect(service.HostName, service.Port);
                }


                if (socket.Connected)
                {
                    _socket = socket;
                    Connect();
                }
                else
                {
                    _connectedState = ConnectedState.ErrorConnecting;
                    if (DidNotConnect != null)
                    {
                        DidNotConnect(this, new Exception("Error opening socket"));
                    }
                }
            }
            catch (Exception exception)
            {
                if (DidNotConnect != null)
                {
                    DidNotConnect(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the connection fails to resolve a Bonjour service
        /// </summary>
        /// <param name="service">The Bonjour service that failed to be resolved</param>
        /// <param name="exception">The reason why the Bonjour service failed to be resolved</param>
        private void _netService_DidNotResolveService(NetService service, DNSServiceException exception)
        {
            _connectedState = ConnectedState.ErrorConnecting;
            if (DidNotConnect != null)
            {
                DidNotConnect(this, exception);
            }
        }

        #endregion

        #region Sending and Receiving Data
        /// <summary>
        /// The synchronous method for receiving data
        /// </summary>
        private void Receive()
        {
            while (true)
            {
                DataInfo dataInfo = null;
                DataHeader header = null;
                DataContent content = null;
                DataFooter footer = null;

                try
                {
                    if (_socket != null)
                    {
                        byte[] infoData = new byte[DataInfo.DataInfoSize];
                        int infoBytesRead = 0;
                        while (infoBytesRead < infoData.Length)
                        {
                            int read = _socket.Receive(infoData, infoBytesRead, infoData.Length - infoBytesRead, SocketFlags.None);
                            if (read != 0)
                            {
                                infoBytesRead += read;
                            }
                        }
                        dataInfo = new DataInfo(infoData);
                        if (dataInfo != null)
                        {
                            byte[] headerData = new byte[dataInfo.HeaderLength];
                            int headerBytesRead = 0;
                            while (headerBytesRead < headerData.Length)
                            {
                                int read = _socket.Receive(headerData, headerBytesRead, headerData.Length - headerBytesRead, SocketFlags.None);
                                if (read != 0)
                                {
                                    headerBytesRead += read;
                                }
                            }
                            header = new DataHeader(headerData);

                            byte[] contentData = new byte[dataInfo.ContentLength];
                            int contentBytesRead = 0;
                            while (contentBytesRead < contentData.Length)
                            {
                                int read = _socket.Receive(contentData, contentBytesRead, contentData.Length - contentBytesRead, SocketFlags.None);
                                if (read != 0)
                                {
                                    contentBytesRead += read;
                                }
                            }
                            content = new DataContent(contentData);

                            byte[] footerData = new byte[dataInfo.FooterLength];
                            int footerBytesRead = 0;
                            while (footerBytesRead < footerData.Length)
                            {
                                int read = _socket.Receive(footerData, footerBytesRead, footerData.Length - footerBytesRead, SocketFlags.None);
                                if (read != 0)
                                {
                                    footerBytesRead += read;
                                }
                                Console.WriteLine("reading footer");
                            }
                            footer = new DataFooter(footerData);
                        }
                    }
                }
                catch { }
                if (dataInfo != null)
                {
                    CommunicationData receivedData = new CommunicationData(dataInfo, header, content, footer);
                    
                    if (dataInfo.DataType == CommunicationDataType.Termination)
                    {
                        Disconnect(false);
                    }
                    else if (DidReceiveData != null)
                    {
                        DidReceiveData(this, receivedData);
                    }
                }
             }
        }

        /// <summary>
        /// Sends a string of a particular encoding
        /// </summary>
        /// <param name="stringToSend">The string to send</param>
        /// <param name="encoding">The encoding of the string to send</param>
        public void SendString(string stringToSend, Encoding encoding)
        {
            SendData(CommunicationData.FromString(stringToSend, encoding));
        }

        /// <summary>
        /// Sends an image
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="name">The name of the image to send</param>
        public void SendImage(Image image, string name)
        {
            SendData(CommunicationData.FromImage(image, name));
        }

        /// <summary>
        /// Sends a file
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        /// <param name="name">The name of the file to send</param>
        public void SendFile(string filePath, string name)
        {
            SendData(CommunicationData.FromFile(filePath, name));
        }

        /// <summary>
        /// Sends a dictionary in a JSON format
        /// </summary>
        /// <param name="dictionary">The dictionary to send</param>
        public void SendDictionary(Dictionary<object, object> dictionary)
        {
            SendData(CommunicationData.FromDictionary(dictionary));
        }

        /// <summary>
        /// Sends an array in a JSON format
        /// </summary>
        /// <param name="array">The array to send</param>
        public void SendArray(List<object> array)
        {
            SendData(CommunicationData.FromArray(array));
        }

        /// <summary>
        /// Sends a JSON string
        /// </summary>
        /// <param name="JSONString">The JSON string to send</param>
        public void SendJSONString(string JSONString)
        {
            if (JSONString == null)
            {
                SendString(JSONString, Encoding.ASCII);
            }
            else
            {
                object JSONObject = null;
                JSONObjectType JSONObjectType = JSONObjectType.Other;

                try
                {
                    JSONObject = new JavaScriptSerializer().DeserializeObject(JSONString);
                }
                catch
                {
                    SendString(JSONString, Encoding.ASCII);
                    return;
                }
                if (DataDetector.IsList(JSONObject))
                {
                    JSONObjectType = JSONObjectType.Array;
                }
                else if (DataDetector.IsDictionary(JSONObject))
                {
                    JSONObjectType = JSONObjectType.Dictionary;
                }
                SendData(CommunicationData.FromJSONData(DataSerializers.StringSerializer.ByteArrayFromString(JSONString, Encoding.ASCII), JSONObjectType));
            }
        }

        /// <summary>
        /// Sends data to a client
        /// </summary>
        /// <param name="dataToSend">The data to send to the client</param>
        public void SendData(byte[] dataToSend)
        {
            SendData(CommunicationData.FromData(dataToSend));
        }

        /// <summary>
        /// Sends data to a client
        /// </summary>
        /// <param name="data">The data to send</param>
        public void SendData(CommunicationData data)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                _socket.Send(data.DataInfo.GetBytes());

                if (data.DataInfo.HeaderLength > 0)
                {
                    _socket.Send(data.DataHeader.GetBytes());
                }

                if (data.DataInfo.ContentLength > 0)
                {
                    _socket.Send(data.DataContent.GetBytes());
                }

                if (data.DataInfo.FooterLength > 0)
                {
                    _socket.Send(data.DataFooter.GetBytes());
                }

                if (data.DataInfo.DataType == CommunicationDataType.Termination)
                {
                    Disconnect(false);
                }
                else if (DidSendData != null)
                {
                    DidSendData(this, data);
                }
            }
            catch (Exception exception)
            {
                if (data.DataInfo.DataType == CommunicationDataType.Termination)
                {
                    Disconnect(false);
                }
                else if (DidNotSendData != null)
                {
                    DidNotSendData(this, data, exception);
                }
            }
        }

        #endregion

        #region Ending

        /// <summary>
        /// Disconnects from a device on the network
        /// </summary>
        /// <param name="sendTerminationMessage">A value indicating whether to send a message to the connection to terminate the connection or whether to disconnect immediately</param>
        public void Disconnect(bool sendTerminationMessage)
        {
            if (sendTerminationMessage)
            {
                SendData(CommunicationData.TerminationCommunicationData);
            }
            else
            {
                _connectedState = ConnectedState.Disconnected;
                _socket.Close();
                _socket = null;
                if (DidDisconnect != null)
                {
                    DidDisconnect(this);
                }
            }
        }

        #endregion
    }
}
