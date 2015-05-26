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
using System.Web.Script.Serialization;
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
        private ConnectionHandler _connectionHandler;

        private ClientState _state;
        private DateTime _startTime;

        private NetService _service;
    
        #endregion 

        #region Properties

        /// <summary>
        /// The backend handler for streams of data and connections
        /// </summary>
        internal ConnectionHandler ConnectionHandler
        {
            get { return _connectionHandler; }
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
            _connectionHandler = new ConnectionHandler();
            _connectionHandler.ConnectionHandlerDidConnect = ConnectionHandlerDidConnect;
            _connectionHandler.ConnectionHandlerDidDisconnect = ConnectionHandlerDidDisconnect;
            _connectionHandler.ConnectionHandlerDidReceiveData = ConnectionHandlerDidReceiveData;
            _connectionHandler.ConnectionHandlerDidSendData = ConnectionHandlerDidSendData;
            _connectionHandler.ConnectionHandlerDidNotSendData = ConnectionHandlerDidNotSendData;

            _state = ClientState.Connecting;
            _connectionHandler.ConnectToSocket(socket);

            _startTime = DateTime.Now;
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private ConnectedClient()
        {

        }

        #endregion

        #region Sending and Receiving

        /// <summary>
        /// Sends data to the client
        /// </summary>
        /// <param name="data">The communication data to send the client. Could be encoded into a image or string etc.</param>
        public void SendData(CommunicationData data)
        {
            _connectionHandler.SendData(data);
        }

        /// <summary>
        /// The delegate method called when the connection handler connects
        /// </summary>
        /// <param name="connectionHandler">The connection handler that connected</param>
        private void ConnectionHandlerDidConnect(ConnectionHandler connectionHandler)
        {
            _state = ClientState.Connected;
            if (_server.ServerDidConnectToClient != null)
            {
                _server.ServerDidConnectToClient(_server, this);
            }
        }

        /// <summary>
        /// The delegate method called when the connection handler disconnects
        /// </summary>
        /// <param name="connectionHandler">The connection handler that disconnected</param>
        private void ConnectionHandlerDidDisconnect(ConnectionHandler connectionHandler)
        {
            _state = ClientState.Disconnected;
            if (_server.ServerDidDisconnectFromClient != null)
            {
                _server.ServerDidDisconnectFromClient(_server, this);
            }
        }

        /// <summary>
        /// The delegate method called when the connection handler receives data
        /// </summary>
        /// <param name="connectionHandler">The connection handler that received the data</param>
        /// <param name="data">The data received</param>
        private void ConnectionHandlerDidReceiveData(ConnectionHandler connectionHandler, CommunicationData data)
        {
            if (_server.ServerDidReceiveDataFromClient != null)
            {
                _server.ServerDidReceiveDataFromClient(_server, this, data);
            }
        }

        /// <summary>
        /// The delegate method called when the connection handler sends data successfully
        /// </summary>
        /// <param name="connectionHandler">The connection handler that sent the data</param>
        /// <param name="data">The data sent</param>
        private void ConnectionHandlerDidSendData(ConnectionHandler connectionHandler, CommunicationData data)
        {
            if (_server.ServerDidSendDataToClient != null)
            {
                _server.ServerDidSendDataToClient(_server, data, this);
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
            if (_server.ServerDidNotSendDataToClient != null)
            {
                _server.ServerDidNotSendDataToClient(_server, data, reason, this);
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
            if (_connectionHandler != null)
            {
                _connectionHandler.Disconnect();
                _connectionHandler = null;
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
                if (_connectionHandler != null) { _connectionHandler.Disconnect(); _connectionHandler = null; }
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
            return "Client: state = " + _state.ToString() + "; socket = " + _connectionHandler.ConnectedSocket.ToString() + "; started connecting = " + _startTime.ToShortTimeString();
        }
    }
}
