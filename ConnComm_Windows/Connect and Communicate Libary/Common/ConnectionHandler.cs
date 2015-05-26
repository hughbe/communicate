using Communicate.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace Communicate.Common
{
    #region Top Level Delegates

    delegate void ConnectionHandlerDidConnect(ConnectionHandler connectionHandler);
    delegate void ConnectionHandlerDidDisconnect(ConnectionHandler connectionHandler);
    delegate void ConnectionHandlerDidReceiveData(ConnectionHandler connectionHandler, CommunicationData data);
    delegate void ConnectionHandlerDidSendData(ConnectionHandler connectionHandler, CommunicationData data);
    delegate void ConnectionHandlerDidNotSendData(ConnectionHandler connectionHandler, CommunicationData data, Exception reason);

    #endregion

    /// <summary>
    /// A class that reads and write to a client's or server's connection
    /// </summary>
    class ConnectionHandler : IDisposable
    {
        #region Private Variables

        private Socket _connectedSocket;
        private TcpClient _tcpClient;

        private ConnectionHandlerDidConnect _connectionHandlerDidConnect;
        private ConnectionHandlerDidDisconnect _connectionHandlerDidDisconnect;
        private ConnectionHandlerDidReceiveData _connectionHandlerDidReceiveData;
        private ConnectionHandlerDidSendData _connectionHandlerDidSendData;
        private ConnectionHandlerDidNotSendData _connectionHandlerDidNotSendData;

        #endregion

        #region Properties

        /// <summary>
        /// The currently connected socket
        /// </summary>
        public Socket ConnectedSocket
        {
            get { return _connectedSocket; }
        }

        /// <summary>
        /// The backend tcp client
        /// </summary>
        public TcpClient TCPClient
        {
            get { return _tcpClient; }
        }

        /// <summary>
        /// The delegate method called when the connection handler connects
        /// </summary>
        public ConnectionHandlerDidConnect ConnectionHandlerDidConnect
        {
            get { return _connectionHandlerDidConnect; }
            set { _connectionHandlerDidConnect = value; }
        }

        /// <summary>
        /// The delegate method called when the connection handler disconnects
        /// </summary>
        public ConnectionHandlerDidDisconnect ConnectionHandlerDidDisconnect
        {
            get { return _connectionHandlerDidDisconnect; }
            set { _connectionHandlerDidDisconnect = value; }
        }

        /// <summary>
        /// The delegate method called when the connection handler receives data
        /// </summary>
        public ConnectionHandlerDidReceiveData ConnectionHandlerDidReceiveData
        {
            get { return _connectionHandlerDidReceiveData; }
            set { _connectionHandlerDidReceiveData = value; }
        }

        /// <summary>
        /// The delegate method called when the connection handler sends data
        /// </summary>
        public ConnectionHandlerDidSendData ConnectionHandlerDidSendData
        {
            get { return _connectionHandlerDidSendData; }
            set { _connectionHandlerDidSendData = value; }
        }

        /// <summary>
        /// The delegate method called when the connection handler fails to send data
        /// </summary>
        public ConnectionHandlerDidNotSendData ConnectionHandlerDidNotSendData
        {
            get { return _connectionHandlerDidNotSendData; }
            set { _connectionHandlerDidNotSendData = value; }
        }

        #endregion

        /// <summary>
        /// Constructs a connection handler for a client
        /// </summary>
        /// <param name="clientInfo">The information about the client to construct a connection handler for</param>
        public ConnectionHandler (ClientInfo clientInfo)
        {
            _tcpClient = new TcpClient(new IPEndPoint(IPAddress.Any, clientInfo.Port));
        }

        /// <summary>
        /// The empty constructor for a connection handler
        /// </summary>
        public ConnectionHandler()
        {

        }
                
        /// <summary>
        /// Connects to a socket
        /// </summary>
        /// <param name="socket">The socket to connect to</param>
        public void ConnectToSocket(Socket socket)
        {
            _connectedSocket = socket;
            StartReceiving();
        }

        #region Sending and Receiving

        /// <summary>
        /// The synchronous method for receiving data
        /// </summary>
        private void Receive()
        {
            try
            {
                while (true)
                {
                    byte[] infoData = new byte[DataInfo.DataInfoSize];
                    int infoBytesRead = 0;
                    while (infoBytesRead < infoData.Length)
                    {
                        int read = _connectedSocket.Receive(infoData, infoBytesRead, infoData.Length - infoBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            infoBytesRead += read;
                        }
                        Console.WriteLine("reading di");
                    }
                    DataInfo dataInfo = new DataInfo(infoData);

                    byte[] headerData = new byte[dataInfo.HeaderLength];
                    int headerBytesRead = 0;
                    while (headerBytesRead < headerData.Length)
                    {
                        int read = _connectedSocket.Receive(headerData, headerBytesRead, headerData.Length - headerBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            headerBytesRead += read;
                        }
                        Console.WriteLine("reading header");
                    }
                    DataHeader header = new DataHeader(headerData);

                    byte[] contentData = new byte[dataInfo.ContentLength];
                    int contentBytesRead = 0;
                    while (contentBytesRead < contentData.Length)
                    {
                        int read = _connectedSocket.Receive(contentData, contentBytesRead, contentData.Length - contentBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            contentBytesRead += read;
                        }
                        Console.WriteLine("reading content");
                    }
                    DataContent content = new DataContent(contentData);

                    byte[] footerData = new byte[dataInfo.FooterLength];
                    int footerBytesRead = 0;
                    while (footerBytesRead < footerData.Length)
                    {
                        int read = _connectedSocket.Receive(footerData, footerBytesRead, footerData.Length - footerBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            footerBytesRead += read;
                        }
                        Console.WriteLine("reading footer");
                    }
                    DataFooter footer = new DataFooter(footerData);

                    CommunicationData receivedData = new CommunicationData(dataInfo, header, content, footer);

                    if (_connectionHandlerDidReceiveData != null)
                    {
                        _connectionHandlerDidReceiveData(this, receivedData);
                    }
                }
            }
            catch { }
        }


        /// <summary>
        /// Starts receiving data
        /// </summary>
        public void StartReceiving()
        {
            if (_connectionHandlerDidConnect != null)
            {
                _connectionHandlerDidConnect(this);
            }
            Thread backgroundThread = new Thread(new ThreadStart(Receive));
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
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
                _connectedSocket.Send(data.DataInfo.GetBytes());

                if (data.DataInfo.HeaderLength > 0)
                {
                    _connectedSocket.Send(data.DataHeader.GetBytes());
                } 

                if (data.DataInfo.ContentLength > 0)
                {
                    _connectedSocket.Send(data.DataContent.GetBytes());
                } 

                if (data.DataInfo.HeaderLength > 0)
                {
                    _connectedSocket.Send(data.DataFooter.GetBytes());
                }

                if (_connectionHandlerDidSendData != null)
                {
                    _connectionHandlerDidSendData(this, data);
                }
            }
            catch (Exception exception)
            {
                if (_connectionHandlerDidNotSendData != null)
                {
                    _connectionHandlerDidNotSendData(this, data, exception);
                }
            }
        }

        #endregion

        #region Ending

        /// <summary>
        /// Disconnects the connected socket
        /// </summary>
        public void Disconnect()
        {
            if (_connectedSocket != null)
            {
                _connectedSocket.Close(10);
                _connectedSocket = null;
            }
            if (_connectionHandlerDidDisconnect != null)
            {
                _connectionHandlerDidDisconnect(this);
            }
        }

        /// <summary>
        /// Dipsposes all connected clients
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects connected clients
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connectedSocket != null) { _connectedSocket.Close(); _connectedSocket = null; }
            }
        }

        #endregion
    }
}
