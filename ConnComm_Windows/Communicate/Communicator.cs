using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communicate.Common;
using Communicate.Listening;
using Communicate.Publishing;
using Communicate.Connecting;
using System.Net.Sockets;
using Communicate.Searching;
using ZeroconfService;
using System.IO;
using System.Drawing;

namespace Communicate
{
    public delegate void StartedPublishing(Communicator communicator);
    public delegate void Published(Communicator communicator);
    public delegate void NotPublished(Communicator communicator, Exception reason);
    public delegate void Unpublished(Communicator communicator);

    public delegate void StartedListening(Communicator communicator);
    public delegate void NotStartedListening(Communicator communicator, Exception reason);
    public delegate void StoppedListening(Communicator communicator);

    public delegate void StartedSearching(Communicator communicator);
    public delegate void FoundServices(Communicator communicator, List<NetService> services);
    public delegate void NotStartedSearching(Communicator communicator, Exception reason);
    public delegate void StoppedSearching(Communicator communicator);

    public delegate void StartedConnecting(Communicator communicator, Connection connection);
    public delegate void Connected(Communicator communicator, Connection connection);
    public delegate void NotConnected(Communicator communicator, Connection connection, Exception reason);
    public delegate void Disconnected(Communicator communicator, Connection connection);

    public delegate void ReceivedData(Communicator communicator, Connection connection, CommunicationData data);
    public delegate void SentData(Communicator communicator, Connection connection, CommunicationData data);
    public delegate void NotSentData(Communicator communicator, Connection connection, CommunicationData data, Exception reason);

    public class Communicator : IDisposable
    {
        #region Private Variables

        private ProtocolInfo _protocol;
        private CommunicatorInfo _communicatorInfo;

        private ListeningManager _listeningManager;
        private PublishingManager _publishingManager;

        private SearchingManager _searchingManager;

        private ConnectionsManager _connectionManager;

        #endregion

        #region Properties

        /// <summary>
        /// The object responsible for listening for incoming connection requests
        /// </summary>
        public ListeningManager ListeningManager
        {
            get { return _listeningManager; }
        }

        /// <summary>
        /// The object responsible for publishing the communicator on the network
        /// </summary>
        public PublishingManager PublishingManager
        {
            get { return _publishingManager; }
        }

        /// <summary>
        /// The object responsible for searching for devices on the network
        /// </summary>
        public SearchingManager SearchingManager
        {
            get { return _searchingManager; }
        }

        /// <summary>
        /// The object responsible for connections and data streams
        /// </summary>
        public ConnectionsManager ConnectionManager
        {
            get { return _connectionManager; }
        }

        /// <summary>
        /// The event called when the communicator starts publishing itself on the network
        /// </summary>
        public event StartedPublishing DidStartPublishing;

        /// <summary>
        /// The event called when the communicator successfully published itself on the network
        /// </summary>
        public event Published DidPublish;

        /// <summary>
        /// The event called when the communicator failed to publish itself on the network
        /// </summary>
        public event NotPublished DidNotPublish;

        /// <summary>
        /// The event called when the communicator unpublished itself from the network
        /// </summary>
        public event Unpublished DidUnpublish;

        /// <summary>
        /// The event called when the communicator starts listening for incoming connection requests
        /// </summary>
        public event StartedListening DidStartListening;

        /// <summary>
        /// The event called when the communicator fails to start listening for incoming connection requests
        /// </summary>
        public event NotStartedListening DidNotStartListening;

        /// <summary>
        /// The event called when the communicator stops listening for incoming connection requests
        /// </summary>
        public event StoppedListening DidStopListening;

        /// <summary>
        /// The event called when the communicator starts searching for devices on the network
        /// </summary>
        public event StartedSearching DidStartSearching;

        /// <summary>
        /// The event called when the communicator discovers devices on the network
        /// </summary>
        public event FoundServices DidDiscoverServices;

        /// <summary>
        /// The event called when the communicator fails to start searching devices on the network
        /// </summary>
        public event NotStartedSearching DidNotStartSearching;

        /// <summary>
        /// The event called when the communicator stops searching for devices on the network
        /// </summary>
        public event StoppedSearching DidStopSearching;
        
        /// <summary>
        /// The event called when the communicator starts connecting to a device on the network
        /// </summary>
        public event StartedConnecting DidStartConnecting;

        /// <summary>
        /// The event called when the communicator successfully connects to a device on the network
        /// </summary>
        public event Connected DidConnect;

        /// <summary>
        /// The event called when the communicator fails to connect to a device on the network
        /// </summary>
        public event NotConnected DidNotConnect;

        /// <summary>
        /// The event called when the communicator disconnects from a device on the network
        /// </summary>
        public event Disconnected DidDisconnect;

        /// <summary>
        /// The event called when the communicator receives data from a device on the network
        /// </summary>
        public event ReceivedData DidReceiveData;

        /// <summary>
        /// The event called when the communicator sends data to a device on the network
        /// </summary>
        public event SentData DidSendData;

        /// <summary>
        /// The event called when the communicator fails to send data to a device on the network
        /// </summary>
        public event NotSentData DidNotSendData;

        #endregion

        #region Starting

        /// <summary>
        /// Constructs a communicator from information about the protocol and communicator
        /// </summary>
        /// <param name="protocol">The information about the network protocol of a communicator</param>
        /// <param name="communicatorInfo">The backend information about a communicator</param>
        public Communicator(ProtocolInfo protocol, CommunicatorInfo communicatorInfo)
        {
            if (protocol == null)
            {
                throw new ArgumentNullException("protocol");
            }
            else if (communicatorInfo == null)
            {
                throw new ArgumentNullException("communicatorInfo");
            }
            else
            {
                _protocol = protocol;
                _communicatorInfo = communicatorInfo;

                _publishingManager = new PublishingManager(_protocol, _communicatorInfo);
                _publishingManager.DidStartPublishing += publishingManager_DidStartPublishing;
                _publishingManager.DidPublish += publishingManager_DidPublish;
                _publishingManager.DidNotPublish += publishingManager_DidNotPublish;
                _publishingManager.DidUnpublish += publishingManager_DidUnpublish;

                _listeningManager = new ListeningManager(_communicatorInfo);
                _listeningManager.DidStartListening += listeningManager_DidStartListening;
                _listeningManager.DidNotStartListening += listeningManager_DidNotStartListening;
                _listeningManager.DidReceiveConnectionRequest += listeningManager_DidReceiveConnectionRequest;
                _listeningManager.DidStopListening += listeningManager_DidStopListening;

                _searchingManager = new SearchingManager(_protocol);
                _searchingManager.DidStartSearching += searchingManager_DidStartSearching;
                _searchingManager.DidFindServices += searchingManager_DidFindServices;
                _searchingManager.DidNotSearch += searchingManager_DidNotSearch;
                _searchingManager.DidStopSearching += searchingManager_DidStopSearching;

                _connectionManager = new ConnectionsManager();
                _connectionManager.DidStartConnecting += connectionManager_DidStartConnecting;
                _connectionManager.DidConnect += connectionManager_DidConnect;
                _connectionManager.DidNotConnect += connectionManager_DidNotConnect;
                _connectionManager.DidDisconnect += connectionManager_DidDisconnect;

                _connectionManager.DidReceiveData += connectionManager_DidReceiveData;
                _connectionManager.DidSendData += connectionManager_DidSendData;
                _connectionManager.DidNotSendData += connectionManager_DidNotSendData;

            }
        }
        
        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private Communicator()
        {

        }

        /// <summary>
        /// Publishes the communicator on the network and starts listening for incoming connections
        /// </summary>
        public void PublishAndStartListening()
        {
            Publish();
            StartListening();
        }

        #region Publishing

        /// <summary>
        /// Publishes the communicator on the network
        /// </summary>
        public void Publish()
        {
            _publishingManager.Publish();
        }

        /// <summary>
        /// The delegate method called when the publishing manager starts to publish the communicator
        /// </summary>
        /// <param name="publishingManager">The publishing manager that started to publish the communicator</param>
        private void publishingManager_DidStartPublishing(PublishingManager publishingManager)
        {
            if (DidStartPublishing != null)
            {
                DidStartPublishing(this);
            }
        }

        /// <summary>
        /// The delegate method called when the publishing manager published the communicator
        /// </summary>
        /// <param name="publishingManager">The publishing manager that published the communicator</param>
        private void publishingManager_DidPublish(PublishingManager publishingManager)
        {
            if (DidPublish != null)
            {
                DidPublish(this);
            }
        }

        /// <summary>
        /// The delegate method called when the publishing manager failed to publish the communicator
        /// </summary>
        /// <param name="publishingManager">The publishing manager that failed to publish the communicator</param>
        /// <param name="reason">The reason why the publishing manager that failed to publish the communicator</param>
        private void publishingManager_DidNotPublish(PublishingManager publishingManager, Exception reason)
        {
            if (DidNotPublish != null)
            {
                DidNotPublish(this, reason);
            }
        }

        /// <summary>
        /// The delegate method called when the publishing manager failed unpublish the communicator
        /// </summary>
        /// <param name="publishingManager">The publishing manager that unpublished the communicator</param>
        private void publishingManager_DidUnpublish(PublishingManager publishingManager)
        {
            if (DidUnpublish != null)
            {
                DidUnpublish(this);
            }
        }

        #endregion

        #region Listening

        /// <summary>
        /// Starts listening for incoming connections
        /// </summary>
        public void StartListening()
        {
            _listeningManager.StartListening();
        }

        /// <summary>
        /// The delegate method called when the listening manager starts to listen for incoming connection requests
        /// </summary>
        /// <param name="listeningManager">The listening manager that started to listen for incoming connection requests</param>
        private void listeningManager_DidStartListening(ListeningManager listeningManager)
        {
            if (DidStartListening != null)
            {
                DidStartListening(this);
            }
        }

        /// <summary>
        /// The delegate method called when the listening manager failed to start listening for incoming connection requests
        /// </summary>
        /// <param name="listeningManager">The listening manager that failed to start listening for incoming connection requests</param>
        /// <param name="reason">The reason why the listening manager failed to start listening for incoming connection requests</param>
        private void listeningManager_DidNotStartListening(ListeningManager listeningManager, Exception reason)
        {
            if (DidNotStartListening != null)
            {
                DidNotStartListening(this, reason);
            }
        }

        /// <summary>
        /// The delegate method called when the listening manager receives an incoming connectionr request from a socket
        /// </summary>
        /// <param name="listeningManager">The listening manager that received the incoming connection request</param>
        /// <param name="connectionSocket">The socket that controls the connection between the communicator and the connector</param>
        private void listeningManager_DidReceiveConnectionRequest(ListeningManager listeningManager, Socket connectionSocket)
        {
            _connectionManager.Connect(connectionSocket);
        }

        /// <summary>
        /// The delegate method called when the listening manager stops listening for incoming connection requests
        /// </summary>
        /// <param name="listeningManager">The listening manager that stopped listening for incoming connection requests</param>
        private void listeningManager_DidStopListening(ListeningManager listeningManager)
        {
            if (DidStopListening != null)
            {
                DidStopListening(this);
            }
        }

        #endregion 

        #region Searching

        /// <summary>
        /// Starts searching for devices on the network
        /// </summary>
        public void StartSearching()
        {
            _searchingManager.StartSearching();
        }

        /// <summary>
        /// The delegate method called when the searching manager starts searching for devices on the network
        /// </summary>
        /// <param name="searchingManager">The searching manager that started to search for devices on the network</param>
        private void searchingManager_DidStartSearching(SearchingManager searchingManager)
        {
            if (DidStartSearching != null)
            {
                DidStartSearching(this);
            }
        }

        /// <summary>
        /// The delegate method called when the searching manager finds some devices on the network
        /// </summary>
        /// <param name="searchingManager">The searching manager that found devices on the network</param>
        /// <param name="services">The list of Bonjour services that the searching manager found</param>
        private void searchingManager_DidFindServices(SearchingManager searchingManager, List<NetService> services)
        {
            if (DidDiscoverServices != null)
            {
                DidDiscoverServices(this, services);
            }
        }

        /// <summary>
        /// The delegate method called when the searching manager fails to start searching for devices on the network
        /// </summary>
        /// <param name="searchingManager">The searching manager that failed to start searching for devices on the network</param>
        /// <param name="reason">The reason why the searching manager that failed to start searching for devices on the network</param>
        private void searchingManager_DidNotSearch(SearchingManager searchingManager, Exception reason)
        {
            if (DidNotStartSearching != null)
            {
                DidNotStartSearching(this, reason);
            }
        }

        /// <summary>
        /// The delegate method called when the searching manager stops searching for devices on the network
        /// </summary>
        /// <param name="searchingManager">The searching manager that stopped searching for devices on the network</param>
        private void searchingManager_DidStopSearching(SearchingManager searchingManager)
        {
            if (DidStopSearching != null)
            {
                DidStopSearching(this);
            }
        }

        #endregion

        #region Connecting

        /// <summary>
        /// Initiates a new connection to a Bonjour service
        /// </summary>
        /// <param name="service">The Bonjour service with which to initialize a connection</param>
        public void ConnectToService(NetService service)
        {
            _connectionManager.Connect(service);
        }

        /// <summary>
        /// The delegate method called when a connection manager starts to connect to a device on the network
        /// </summary>
        /// <param name="connectionManager">The connection manager that started to connect to a device on the network</param>
        /// <param name="connection">The connection that represents the device on the network to which the connection manager is connecting</param>
        private void connectionManager_DidStartConnecting(ConnectionsManager connectionManager, Connection connection)
        {
            if (DidStartConnecting != null)
            {
                DidStartConnecting(this, connection);
            }
        }

        /// <summary>
        /// The delegate method called when a connection manager successfully connects to a device on the network
        /// </summary>
        /// <param name="connectionManager">The connection manager that successfully connected to a device on the network</param>
        /// <param name="connection">The connection that represents the device on the network to which the connection manager connected</param>
        private void connectionManager_DidConnect(ConnectionsManager connectionManager, Connection connection)
        {
            if (DidConnect != null)
            {
                DidConnect(this, connection);
            }
        }

        /// <summary>
        /// The delegate method called when a connection manager fails to connect to a device on the network
        /// </summary>
        /// <param name="connectionManager">The connection manager that failed to connect to a device on the network</param>
        /// <param name="connection">The connection that represents the device on the network to which the connection manager failed to connect</param>
        /// <param name="reason">The reason why the connection manager failed to connect to a device on the network</param>
        private void connectionManager_DidNotConnect(ConnectionsManager connectionManager, Connection connection, Exception reason)
        {
            if (DidNotConnect != null)
            {
                DidNotConnect(this, connection, reason);
            }
        }

        /// <summary>
        /// The delegate method called when a connection manager disconnects from a device on the network
        /// </summary>
        /// <param name="connectionManager">The connection manager that disconnected from a device on the network</param>
        /// <param name="connection">The connection that represents the device on the network from which the connection manager disconnected</param>
        private void connectionManager_DidDisconnect(ConnectionsManager connectionManager, Connection connection)
        {
            if (DidDisconnect != null)
            {
                DidDisconnect(this, connection);
            }
        }

        #endregion

        #endregion

        #region Sending and Receiving Data

        /// <summary>
        /// The delegate method called when the connection manager successfully sends data to a device on the network
        /// </summary>
        /// <param name="connectionManager">The connection manager that sent data to a device on the network</param>
        /// <param name="connection">The connection that represents the device on the network to which the connection manager sent data</param>
        /// <param name="data">The data sent to a device on the network</param>
        private void connectionManager_DidSendData(ConnectionsManager connectionManager, Connection connection, CommunicationData data)
        {
            if (DidSendData != null)
            {
                DidSendData(this, connection, data);
            }
        }

        /// <summary>
        /// The delegate method called when the connection manager fails to send data to a device on the network
        /// </summary>
        /// <param name="connectionManager">The connection manager that failed to send data to a device on the network</param>
        /// <param name="connection">The connection that represents the device on the network to which the connection manager failed to send data</param>
        /// <param name="data">The data that failed to be sent to a device on the network</param>
        /// <param name="reason">The reason why the data  failed to be sent to a device on the network</param>
        private void connectionManager_DidNotSendData(ConnectionsManager connectionManager, Connection connection, CommunicationData data, Exception reason)
        {
            if (DidNotSendData != null)
            {
                DidNotSendData(this, connection, data, reason);
            }
        }

        /// <summary>
        /// The delegate method called when the connection manager receives data from a device on the network
        /// </summary>
        /// <param name="connectionManager">The connection manager that received data from a device on the network</param>
        /// <param name="connection">The connection that represents the device on the network from which the connection manager received data</param>
        /// <param name="data">The data received from a device on the network</param>
        private void connectionManager_DidReceiveData(ConnectionsManager connectionManager, Connection connection, CommunicationData data)
        {
            if (DidReceiveData != null)
            {
                DidReceiveData(this, connection, data);
            }
        }

        #endregion

        #region Ending

        /// <summary>
        /// Stops all of the communicator's activities;
        /// </summary>
        public void Stop()
        {
            Unpublish();
            StopListening();
            StopSearching();
            Disconnect();
        }

        /// <summary>
        /// Unpublishes the communicator from the network and stops listening for incoming connection requests
        /// </summary>
        public void UnpublishAndStopListening()
        {
            Unpublish();
            StopListening();
        }

        /// <summary>
        /// Unpublishes the communicator from the network
        /// </summary>
        public void Unpublish()
        {
            _publishingManager.Unpublish();
        }

        /// <summary>
        /// Stops listening for incoming connection requests
        /// </summary>
        public void StopListening()
        {
            _listeningManager.StopListening();
        }

        /// <summary>
        /// Stop searching for devices on the network
        /// </summary>
        public void StopSearching()
        {
            _searchingManager.StopSearching();
        }
        
        /// <summary>
        /// Disconnects the communicator from all devices that it is connected to on the network
        /// </summary>
        public void Disconnect()
        {
            _connectionManager.Disconnect();
        }

        /// <summary>
        /// Dipsposes all objects managed by the communicator
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects managed by the communicator
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_publishingManager != null) { _publishingManager.Dispose(); _publishingManager = null; }
                if (_listeningManager != null) { _listeningManager.Dispose(); _listeningManager = null; }
                if (_searchingManager != null) { _searchingManager.Dispose(); _searchingManager = null; }
            }
        }

        #endregion
    }
}
