using Communicate.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ZeroconfService;

namespace Communicate.Connections
{
    internal delegate void ManagerStartedConnecting(ConnectionsManager connectionManager, Connection connection);
    internal delegate void ManagerConnected(ConnectionsManager connectionManager, Connection connection);
    internal delegate void ManagerNotConnected(ConnectionsManager connectionManager, Connection connection, Exception reason);
    internal delegate void ManagerDisconnected(ConnectionsManager connectionManager, Connection connection);

    internal delegate void ManagerReceivedData(ConnectionsManager connectionManager, Connection connection, CommunicationData data);
    internal delegate void ManagerSentData(ConnectionsManager connectionManager, Connection connection, CommunicationData data);
    internal delegate void ManagerNotSentData(ConnectionsManager connectionManager, Connection connection, CommunicationData data, Exception reason);

    public class ConnectionsManager
    {
        #region Private Variables

        private List<Connection> _connections;

        #endregion

        #region Properties

        /// <summary>
        /// The list of devices the communicator is currently connected to
        /// </summary>
        internal List<Connection> Connections
        {
            get { return _connections; }
        }

        /// <summary>
        /// The event called when a connection starts to connect to a device on the network
        /// </summary>
        internal event ManagerStartedConnecting DidStartConnecting;

        /// <summary>
        /// The event called when a connection successfully connects to a device on the network
        /// </summary>
        internal event ManagerConnected DidConnect;

        /// <summary>
        /// The event called when a connection fails to connect to a device on the network
        /// </summary>
        internal event ManagerNotConnected DidNotConnect;

        /// <summary>
        /// The event called when a connection disconnects from a device on the network
        /// </summary>
        internal event ManagerDisconnected DidDisconnect;

        /// <summary>
        /// The event called when a connection receives data from a device on the network
        /// </summary>
        internal event ManagerReceivedData DidReceiveData;

        /// <summary>
        /// The event called when a connection sends data to a device on the network
        /// </summary>
        internal event ManagerSentData DidSendData;

        /// <summary>
        /// The event called when a connection fails to send data to a device on the network
        /// </summary>
        internal event ManagerNotSentData DidNotSendData;

        #endregion

        #region Starting

        /// <summary>
        /// The empty constructor for a connections manager
        /// </summary>
        internal ConnectionsManager()
        {
            _connections = new List<Connection>();
        }

        /// <summary>
        /// Initializes a connection to a particular socket
        /// </summary>
        /// <param name="socket">The socket with which to construct a connection</param>
        public void Connect(Socket socket)
        {
            Connection connection = new Connection(socket);
            StartConnecting(connection);
        }

        public void Connect(NetService service)
        {
            Connection connection = new Connection(service);
            StartConnecting(connection);
        }

        /// <summary>
        /// A helper method that sets up a given connection 
        /// </summary>
        /// <param name="connection"></param>
        private void StartConnecting(Connection connection)
        {
            connection.DidStartConnecting += connection_DidStartConnecting;
            connection.DidConnect += connection_DidConnect;
            connection.DidNotConnect += connection_DidNotConnect;
            connection.DidDisconnect += connection_DidDisconnect;

            connection.DidReceiveData += connection_DidReceiveData;
            connection.DidSendData += connection_DidSendData;
            connection.DidNotSendData += connection_DidNotSendData;

            connection.Connect();
        }
        
        /// <summary>
        /// The delegate method called when a connection starts connecting to a device on the network
        /// </summary>
        /// <param name="connection">The connection that started connecting to a device on the network</param>
        private void connection_DidStartConnecting(Connection connection)
        {
            if (DidStartConnecting != null)
            {
                DidStartConnecting(this, connection);
            }
        }

        /// <summary>
        /// The delegate method called when a connection successfully connected to a device on the network
        /// </summary>
        /// <param name="connection">The connection that connected to a device on the network</param>
        private void connection_DidConnect(Connection connection)
        {
            _connections.Add(connection);
            if (DidConnect != null)
            {
                DidConnect(this, connection);
            }
        }

        /// <summary>
        /// The delegate method called when a connection fails to connect to a device on the network
        /// </summary>
        /// <param name="connection">The connection that failed to connect to a device on the network</param>
        /// <param name="reason">The reason why connection that failed to connect to a device on the network</param>
        private void connection_DidNotConnect(Connection connection, Exception reason)
        {
            if (DidNotConnect != null)
            {
                DidNotConnect(this, connection, reason);
            }
        }

        /// <summary>
        /// The delegate method called when a connection disconnects from a device on the network
        /// </summary>
        /// <param name="connection">The connection that disconnected from a device on the network</param>
        private void connection_DidDisconnect(Connection connection)
        {
            if (_connections.Contains(connection))
            {
                _connections.Remove(connection);
            }
            if (DidDisconnect != null)
            {
                DidDisconnect(this, connection);
            }
        }

        #endregion

        #region Sending and Receiving Data

        #region To All Connections

        /// <summary>
        /// Sends a string to all connections
        /// </summary>
        /// <param name="stringToSend">The string to send using ASCII encoding</param>
        public void SendString(string stringToSend)
        {
            SendString(stringToSend, Encoding.ASCII);
        }

        /// <summary>
        /// Sends a string of a particular encoding to all connections
        /// </summary>
        /// <param name="stringToSend">The string to send using ASCII encoding</param>
        /// <param name="encoding">The encoding of the string to send to the connection</param>
        public void SendString(string stringToSend, Encoding encoding)
        {
            SendString(stringToSend, encoding, null);
        }

        /// <summary>
        /// Sends an untitled image to all connections
        /// </summary>
        /// <param name="image">The image to send</param>
        public void SendImage(Image image)
        {
            SendImage(image, "Untitled");
        }

        /// <summary>
        /// Sends an image to all connections
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="name">The name of the image to send</param>
        public void SendImage(Image image, string name)
        {
            SendImage(image, name, null);
        }

        /// <summary>
        /// Sends a file to all connections
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        public void SendFile(string filePath)
        {
            SendFile(filePath, Path.GetFileName(filePath));
        }

        /// <summary>
        /// Sends a file to all connections
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        /// <param name="name">The name of the file to send</param>
        public void SendFile(string filePath, string name)
        {
            SendFile(filePath, name, null);
        }
        /// <summary>
        /// Sends a dictionary encoded in JSON to all connections
        /// </summary>
        /// <param name="dictionary">The dictionary to send</param>
        public void SendDictionary(Dictionary<object, object> dictionary)
        {
            SendDictionary(dictionary, null);
        }

        /// <summary>
        /// Sends an array encoded in JSON to all connections
        /// </summary>
        /// <param name="array">The array to send</param>
        public void SendArray(List<object> array)
        {
            SendArray(array, null);
        }

        /// <summary>
        /// Sends a JSON string to all connections
        /// </summary>
        /// <param name="JSONString">The JSON string to send</param>
        public void SendJSONString(string JSONString)
        {
            SendJSONString(JSONString, null);
        }

        /// <summary>
        /// Sends data to all connections
        /// </summary>
        /// <param name="dataToSend">The data to send</param>
        public void SendData(byte[] dataToSend)
        {
            SendData(dataToSend, null);
        }

        /// <summary>
        /// Sends communication to all connections
        /// </summary>
        /// <param name="data">The communication data to send</param>
        public void SendData(CommunicationData data)
        {
            SendData(data, null);
        }

        #endregion

        #region To Individual Connections

        /// <summary>
        /// Sends a string to a connection
        /// </summary>
        /// <param name="connection">The connection to send the string</param>
        /// <param name="stringToSend">The string to send to the connection using ASCII encoding</param>
        public void SendString(string stringToSend, Connection connection)
        {
            SendString(stringToSend, Encoding.ASCII, connection);
        }

        /// <summary>
        /// Sends a string of a particular encoding to a connection
        /// </summary>
        /// <param name="stringToSend">The string to send to the connection</param>
        /// <param name="encoding">The encoding of the string to send to the connection</param>
        /// <param name="connection">The connection to send the string to</param>
        public void SendString(string stringToSend, Encoding encoding, Connection connection)
        {
            if (connection != null)
            {
                connection.SendString(stringToSend, encoding);
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendString(stringToSend, encoding);
                }
            }
        }

        /// <summary>
        /// Sends an untitled image to a connection
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="connection">The connection to send the image to</param>
        public void SendImage(Image image, Connection connection)
        {
            if (connection != null)
            {
                connection.SendImage(image, "Untitled");
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendImage(image, "Untitled");
                }
            }
        }

        /// <summary>
        /// Sends an image to a connection
        /// </summary>
        /// <param name="image">The image to send</param>
        /// <param name="name">The name of the image to send</param>
        /// <param name="connection">The connection to send the image to</param>
        public void SendImage(Image image, string name, Connection connection)
        {
            if (connection != null)
            {
                connection.SendImage(image, name);
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendImage(image, name);
                }
            }
        }

        /// <summary>
        /// Sends a file to a connection
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        /// <param name="connection">The connection to send the file to</param>
        public void SendFile(string filePath, Connection connection)
        {
            SendFile(filePath, connection);
        }

        /// <summary>
        /// Sends a file to a connection
        /// </summary>
        /// <param name="filePath">The path of the file to send</param>
        /// <param name="connection">The connection to send the file to</param>
        public void SendFile(string filePath, string name, Connection connection)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            if (DataDetector.IsValidImage(bytes))
            {
                SendImage(Image.FromFile(filePath), name, connection);
            }
            else
            {
                if (connection != null)
                {
                    connection.SendFile(filePath, name);
                }
                else
                {
                    foreach (Connection otherConnection in _connections)
                    {
                        otherConnection.SendFile(filePath, name);
                    }
                }
            }
        }

        /// <summary>
        /// Sends a dictionary encoded in JSON to a connection
        /// </summary>
        /// <param name="dictionary">The dictionary to send</param>
        /// <param name="connection">The connection to send the dictionary to</param>
        public void SendDictionary(Dictionary<object, object> dictionary, Connection connection)
        {
            if (connection != null)
            {
                connection.SendDictionary(dictionary);
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendDictionary(dictionary);
                }
            }
        }

        /// <summary>
        /// Sends an array encoded in JSON to a connection
        /// </summary>
        /// <param name="array">The array to send</param>
        /// <param name="connection">The connection to send the array to</param>
        public void SendArray(List<object> array, Connection connection)
        {
            if (connection != null)
            {
                connection.SendArray(array);
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendArray(array);
                }
            }
        }

        /// <summary>
        /// Sends a JSON string to a connection
        /// </summary>
        /// <param name="JSONString">The JSON string to send</param>
        /// <param name="connection">The connection to send the JSON string to</param>
        public void SendJSONString(string JSONString, Connection connection)
        {
            if (connection != null)
            {
                connection.SendJSONString(JSONString);
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendJSONString(JSONString);
                }
            }
        }

        /// <summary>
        /// Sends data to a connection
        /// </summary>
        /// <param name="dataToSend">The data to send to the connection</param>
        /// <param name="connection">The connection to send the data to</param>
        public void SendData(byte[] dataToSend, Connection connection)
        {
            if (connection != null)
            {
                connection.SendData(dataToSend);
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendData(dataToSend);
                }
            }
        }

        /// <summary>
        /// Sends data to a connection
        /// </summary>
        /// <param name="data">The data to send to the connection</param>
        /// <param name="connection">The connection to send the data to</param>
        private void SendData(CommunicationData data, Connection connection)
        {
            if (connection != null)
            {
                connection.SendData(data);
            }
            else
            {
                foreach (Connection otherConnection in _connections)
                {
                    otherConnection.SendData(data);
                }
            }
        }

        #endregion

        #region Delegates

        /// <summary>
        /// The delegate method called when a connection receives data from a device on the network
        /// </summary>
        /// <param name="connection">The connection that received data from a device on the network</param>
        /// <param name="data">The data that was received</param>
        private void connection_DidReceiveData(Connection connection, CommunicationData data)
        {
            if (DidReceiveData != null)
            {
                DidReceiveData(this, connection, data);
            }
        }

        /// <summary>
        /// The delegate method called when a connection successfully sends data to a device on the network
        /// </summary>
        /// <param name="connection">The connection that sent data to a device on the network</param>
        /// <param name="data">The data that was sent</param>
        private void connection_DidSendData(Connection connection, CommunicationData data)
        {
            if (DidSendData != null)
            {
                DidSendData(this, connection, data);
            }
        }

        /// <summary>
        /// The delegate method called when a connection fils to send data to a device on the network
        /// </summary>
        /// <param name="connection">The connection that failed to send data to a device on the network</param>
        /// <param name="data">The data that failed to be sent</param>
        private void connection_DidNotSendData(Connection connection, CommunicationData data, Exception reason)
        {
            if (DidNotSendData != null)
            {
                DidNotSendData(this, connection, data, reason);
            }
        }

        #endregion

        #endregion

        #region Ending

        /// <summary>
        /// Disconnects all connections
        /// </summary>
        public void Disconnect()
        {
            foreach(Connection connection in _connections.ToList()) 
            {
                connection.Disconnect(true);
            }
        }

        #endregion
    }
}
