using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;

namespace Communicate
{
    public abstract class BaseCommunicator : IDisposable
    {
        protected BaseCommunicator(CommunicatorInformation communicatorInformation, Protocol protocol)
        {
            RegisteredObjects.RegisterObjects();
            if (communicatorInformation == null)
            {
                throw new ArgumentNullException(nameof(communicatorInformation));
            }
            if (protocol == null)
            {
                throw new ArgumentNullException(nameof(protocol));
            }

            CommunicatorInformation = communicatorInformation;
            Protocol = protocol;

            ConnectionListener = new TcpListener(IPAddress.Any, CommunicatorInformation.Port);
        }

        public CommunicatorInformation CommunicatorInformation { get; }
        public Protocol Protocol { get; }

        public Collection<TxtRecord> TxtRecords { get; set; }
        
        public event EventHandler DidUpdatePublishedState;

        public event EventHandler DidUpdateSearchingState;
        public event EventHandler DidUpdateServices;

        public event EventHandler DidUpdateListeningState;

        public event EventHandler<ConnectionEventArgs> DidUpdateConnectionState;
        public event EventHandler<ConnectionEventArgs> DidUpdateTxtRecords;
        public event EventHandler<ConnectionEventArgs> DidUpdateInformation;

        public Func<Connection, bool> ShouldConnectToConnection { get; set; }

        public event EventHandler<ConnectionEventArgs> DidUpdateReceivingData;
        public event EventHandler<ConnectionEventArgs> DidUpdateSendingData;

        public State PublishedState { get; private set; } = State.Ready;
        public CommunicatorException PublishingException { get; private set; }

        public State SearchingState { get; private set; } = State.Ready;
        public CommunicatorException SearchingException { get; private set; }

        public Collection<Connection> DiscoveredServices { get; } = new Collection<Connection>();

        public ConnectionCollection Connections { get; } = new ConnectionCollection();

        public State ListeningState { get; private set; } = State.Ready;
        public CommunicatorException ListeningException { get; private set; }
        private TcpListener ConnectionListener { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ConnectionListener != null)
                {
                    ConnectionListener.Stop();
                    ConnectionListener = null;
                }
            }
        }

        public void StopAll()
        {
            RemoveFromNetwork();
            StopSearchingForDevices();
            StopListeningForConnections();
            Connections.DisconnectAll(true);
        }
        protected void UpdatePublishedState(State newState)
        {
            if (newState == PublishedState)
            {
                return;
            }
            PublishedState = newState;
            DidUpdatePublishedState?.Invoke(this, EventArgs.Empty);
        }

        protected void HandlePublishingException(CommunicatorErrorCode errorCode, Exception innerException)
        {
            PublishingException = new CommunicatorException(errorCode, innerException);
            UpdatePublishedState(State.Error);
        }

        protected abstract void HandlePublish();
        protected abstract void HandleStopPublishing();

        public void PublishOnNetwork()
        {
            if (PublishedState == State.Starting || PublishedState == State.Started)
            {
                return;
            }

            UpdatePublishedState(State.Starting);

            HandlePublish();
        }

        public void RemoveFromNetwork()
        {
            if (PublishedState != State.Starting && PublishedState != State.Started)
            {
                return;
            }

            HandleStopPublishing();

            UpdatePublishedState(State.Stopped);
        }

        public void RepublishOnNetwork()
        {
            RemoveFromNetwork();
            PublishOnNetwork();
        }

        protected void UpdateSearchingState(State newState)
        {
            if (newState == SearchingState)
            {
                return;
            }
            SearchingState = newState;
            DidUpdateSearchingState?.Invoke(this, EventArgs.Empty);
        }

        protected void HandleSearchingException(CommunicatorErrorCode errorCode, Exception innerException)
        {
            SearchingException = new CommunicatorException(errorCode, innerException);
            UpdateSearchingState(State.Error);
        }

        protected abstract void HandleStartSearching();
        protected abstract void HandleStopSearching();

        public abstract Collection<TxtRecord> TxtRecordsFromData(byte[] data);
        public abstract byte[] DataFromTxtRecords(Collection<TxtRecord> txtRecords);

        protected void AddService(Connection service)
        {
            if (DiscoveredServices.Contains(service))
            {
                return;
            }
            DiscoveredServices.Add(service);
            DidUpdateServices?.Invoke(this, EventArgs.Empty);
        }

        protected void RemoveService(Connection service)
        {
            if (!DiscoveredServices.Contains(service))
            {
                return;
            }
            DiscoveredServices.Remove(service);
            DidUpdateServices?.Invoke(this, EventArgs.Empty);
        }

        public void StartSearchingForDevices()
        {
            if (SearchingState == State.Started)
            {
                return;
            }

            HandleStartSearching();
            UpdateSearchingState(State.Started);
        }

        public void StopSearchingForDevices()
        {
            if (SearchingState != State.Started)
            {
                return;
            }

            HandleStartSearching();

            DiscoveredServices.Clear();
            UpdateSearchingState(State.Stopped);
        }

        public void RestartSearchingForDevices()
        {
            StopSearchingForDevices();
            StartSearchingForDevices();
        }

        protected void UpdateListeningState(State newState)
        {
            if (newState == ListeningState)
            {
                return;
            }
            ListeningState = newState;
            DidUpdateListeningState?.Invoke(this, EventArgs.Empty);
        }

        protected void HandleListeningException(CommunicatorErrorCode errorCode, Exception innerException)
        {
            ListeningException = new CommunicatorException(errorCode, innerException);
            UpdateListeningState(State.Error);
        }

        protected void HandleStartListening()
        {
            ConnectionListener.Start();
            ConnectionListener.BeginAcceptSocket(AcceptSocketCallback, ConnectionListener);
        }

        protected void HandleStopListening()
        {
            ConnectionListener.Stop();
        }

        public void StartListeningForConnections()
        {
            if (ListeningState == State.Started)
            {
                return;
            }
            try
            {
                HandleStartListening();

                UpdateListeningState(State.Started);
            }
            catch (SocketException exception)
            {
                HandleListeningException(CommunicatorErrorCode.ListeningSocketCouldNotOpen, exception);
            }
            catch (ObjectDisposedException exception)
            {
                HandleListeningException(CommunicatorErrorCode.ListeningSocketClosed, exception);
            }
        }

        public void StopListeningForConnections()
        {
            if (ListeningState != State.Started)
            {
                return;
            }

            HandleStopListening();

            UpdateListeningState(State.Stopped);
        }

        public void RestartListeningForConnection()
        {
            StopListeningForConnections();
            StartListeningForConnections();
        }

        private void AcceptSocketCallback(IAsyncResult asyncResult)
        {
            var listener = (TcpListener)asyncResult.AsyncState;
            try
            {
                ConnectTo(listener.EndAcceptSocket(asyncResult));
            }
            catch (SocketException)
            {
            }
            finally
            {
                listener.BeginAcceptSocket(AcceptSocketCallback, listener);
            }
        }

        public void ConnectTo(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            ConnectTo(new Connection(socket));
        }

        public void ConnectTo(IPAddress address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }
            if (port < 0 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), port, "The port should be between 0 and 65535");
            }
            ConnectTo(new IPEndPoint(address, port));
        }

        public void ConnectTo(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }
            ConnectTo(new Connection(endPoint));
        }

        public void ConnectTo(Connection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }
            if (Connections.Contains(connection))
            {
                return;
            }

            var shouldConnect = ShouldConnectToConnection?.Invoke(connection) ?? true;

            if (!shouldConnect)
            {
                connection.HandleException(CommunicatorErrorCode.ConnectionRejected, null);
                return;
            }

            SetupConnection(connection);

            connection.Connect();
        }

        private void SetupConnection(Connection connection)
        {
            connection.DidUpdateState += (baseConnection, eventArgs) =>
            {
                if (Connections.Contains(connection) && (connection.State == ConnectionState.Connected || connection.State == ConnectionState.Connecting))
                {
                    return;
                }

                if (connection.State == ConnectionState.Connected)
                {
                    Connections.Add(connection);
                }
                else if (connection.State == ConnectionState.Disconnected && Connections.Contains(connection))
                {
                    Connections.Remove(connection);
                }

                DidUpdateConnectionState?.Invoke(this, new ConnectionEventArgs(connection));
            };

            connection.DidUpdateTxtRecords += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs(connection);
                connection.TxtRecords = TxtRecordsFromData(connection.TxtRecordsData);
                DidUpdateTxtRecords?.Invoke(this, eventArgs);
            };

            connection.DidUpdateInformation += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs(connection);
                DidUpdateInformation?.Invoke(this, eventArgs);
            };

            connection.DidUpdateReceivingData += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs(connection, dataArgs.Data,
                    dataArgs.DataComponent, dataArgs.ActionState, dataArgs.Progress);
                DidUpdateReceivingData?.Invoke(this, eventArgs);
            };

            connection.DidUpdateSendingData += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs(connection, dataArgs.Data,
                    dataArgs.DataComponent, dataArgs.ActionState, dataArgs.Progress);
                DidUpdateSendingData?.Invoke(this, eventArgs);
            };
        }

        public void SendData(CommunicationData data, Connection connection)
        {
            if (connection != null)
            {
                connection.Send(data);
            }
            else
            {
                Connections.SendToAll(data);
            }
        }

        public abstract string SerializeProtocolType();
    }
}