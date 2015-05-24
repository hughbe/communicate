using Communicate.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ZeroconfService;

namespace Communicate.Server
{
    /// <summary>
    /// A class representing a client connected to a central server
    /// </summary>
    public class ConnectedClient : IDisposable
    {

        #region Private Variables

        private Server _server;
        private Socket _socket;

        private ClientState _state;
        private DateTime _startTime;

        private NetService _service;
    
        #endregion 

        #region Properties

        /// <summary>
        /// The backend information about the socket used to send and receive contentData
        /// </summary>
        public Socket Socket
        {
            get { return _socket; }
        }

        /// <summary>
        /// The server to which the client is connected
        /// </summary>
        public Server Server
        {
            get { return _server; }
        }

        /// <summary>
        /// The time when the server began to connect to the client
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
        }

        /// <summary>
        /// A value that indicates the state of the client
        /// </summary>
        public ClientState State
        {
            get { return _state; }
            set { _state = value; }
        }

        #endregion

        #region Starting

        /// <summary>
        /// The default constructor for the connected client object
        /// </summary>
        /// <param name="server">The server to which the client is connected</param>
        /// <param name="socket">The backend information about the socket used to send and receive contentData</param>
        public ConnectedClient (Server server, Socket socket)
        {
            _server = server;
            _socket = socket;
            _startTime = DateTime.Now;
            _state = ClientState.NotConnected;
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        public ConnectedClient()
        {

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
                        int read = _socket.Receive(headerData, headerBytesRead, headerData.Length - headerBytesRead, SocketFlags.None);
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
                        int read = _socket.Receive(contentData, contentBytesRead, contentData.Length - contentBytesRead, SocketFlags.None);
                        if (read != 0)
                        {
                            contentBytesRead += read;
                        }
                    }

                    byte[] footerData = new byte[footerLength];
                    int footerBytesRead = 0;
                    while (footerBytesRead < footerData.Length)
                    {
                        int read = _socket.Receive(footerData, footerBytesRead, footerData.Length - footerBytesRead, SocketFlags.None);
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

                    if (_server.ServerDidReceiveDataFromClient != null)
                    {
                        _server.ServerDidReceiveDataFromClient(_server, this, contentData, footerContent, receivingDataType);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        /// <summary>
        /// Starts receiving contentData
        /// </summary>
        public void StartReceiving()
        {
            Thread backgroundThread = new Thread(new ThreadStart(Receive));
            backgroundThread.IsBackground = true;
            backgroundThread.Start();
        }

        /// <summary>
        /// Sends contentData to the client
        /// </summary>
        /// <param name="dataToSend">The byte array of contentData sent. Could be encoded into a image or string etc.</param>
        public void SendData(Collection<byte[]> dataList)
        {

            if (dataList == null || dataList.Count < 3)
            {
                return;
            }
            byte[] header = dataList[0];
            byte[] bytes = dataList[1];
            byte[] footer = dataList[2];

            if (_state == ClientState.Connected)
            {
                try
                {
                    _socket.Send(header);
                    _socket.Send(bytes);
                    _socket.Send(footer);
                    if (_server.ServerDidSendDataToClient!= null)
                    {
                        _server.ServerDidSendDataToClient(_server, bytes, this);
                    }
                }
                catch (Exception exception)
                {
                    if (_server.ServerDidNotSendDataToClient != null)
                    {
                        _server.ServerDidNotSendDataToClient(_server, bytes, exception, this);
                    }
                }
            }
            else if (_server.ServerDidNotSendDataToClient != null)
            {
                _server.ServerDidNotSendDataToClient(_server, bytes, new Exception("Server is not connected to the client"), this);
            }
        }

        #endregion

        #region Ending

        /// <summary>
        /// Disconnects the client from the server
        /// </summary>
        public void Disconnect()
        {
            State = ClientState.NotConnected;
            if (_socket != null)
            {
                _socket.Close();
                _socket = null;
            }
            if (_service != null)
            {
                _service.Dispose();
                _service = null;
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
                if (_socket != null) { _socket.Close(); _socket = null; }
                if (_service != null) { _service.Dispose(); _service = null; }
            }
        }

        #endregion

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the connected client in a readable format</returns>
        public override string ToString()
        {
            return "Client: state = " + _state.ToString() + "; socket = " + _socket.ToString() + "; started connecting = " + _startTime.ToShortTimeString();
        }
    }
}
