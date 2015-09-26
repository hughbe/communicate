using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Communicate
{
    public abstract class BaseCommunicator<TProtocol, TTxtRecords, TConnection> : IDisposable where TProtocol: BaseProtocol where TConnection : BaseConnection<TTxtRecords>, new() where TTxtRecords : BaseTxtRecords
    {
        public delegate void UpdatedConnectionState(object sender, ConnectionEventArgs<TConnection, TTxtRecords> e);

        public delegate void UpdatedData(object sender, ConnectionEventArgs<TConnection, TTxtRecords> e);

        public delegate void UpdatedState(object sender);

        public delegate int UpdatePercentage(object sender, ConnectionEventArgs<TConnection, TTxtRecords> e);
        public delegate bool ConnectionShould(object sender, ConnectionEventArgs<TConnection, TTxtRecords> e);

        public Connections<TConnection, TTxtRecords> Connections { get; } = new Connections<TConnection, TTxtRecords>();

        public TTxtRecords TxtRecords { get; protected set; }

        public void UpdateTxtRecords(TTxtRecords txtRecords)
        {
            TxtRecords = txtRecords;
        }

        protected BaseCommunicator(CommunicatorInformation communicatorInformation, TProtocol protocol)
        {
            CommunicatorInformation = communicatorInformation;
            Protocol = protocol;

            ConnectionListener = new TcpListener(IPAddress.Any, CommunicatorInformation.Port);
        }

        public CommunicatorInformation CommunicatorInformation { get; }
        public TProtocol Protocol { get; }

        public State ListeningState { get; private set; } = State.Ready;
        public Exception ListeningException { get; private set; }
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

        public event UpdatedState DidUpdateListeningState;

        public event UpdatedConnectionState DidUpdateConnectionState;
        public event UpdatedConnectionState DidUpdateTxtRecords;

        public event ConnectionShould ShouldConnectToConnection;

        public event UpdatePercentage DelegateReceivingUpdatePercentage;
        public event UpdatedData DidUpdateReceivingData;

        public event UpdatePercentage DelegateSendingUpdatePercentage;
        public event UpdatedData DidUpdateSendingData;

        public virtual void Stop()
        {
            StopListeningForConnections();
            Connections.DisconnectAll(true);
        }

        protected void UpdateListeningState(State newState)
        {
            if (newState == ListeningState)
            {
                return;
            }
            ListeningState = newState;
            DidUpdateListeningState?.Invoke(this);
        }

        protected void HandleListeningException(Exception exception)
        {
            ListeningException = exception;
            UpdateListeningState(State.Error);
        }

        protected virtual void HandleStartListening()
        {
            ConnectionListener.Start();
            ConnectionListener.BeginAcceptSocket(AcceptSocketCallback, ConnectionListener);
        }

        protected virtual void HandleStopListening()
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
            catch (Exception exception)
            {
                HandleListeningException(exception);
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
            var listener = (TcpListener) asyncResult.AsyncState;
            try
            {
                ConnectTo(listener.EndAcceptSocket(asyncResult));
            }
            catch { }
            finally
            {
                listener.BeginAcceptSocket(AcceptSocketCallback, listener);
            }
        }

        public void ResolveConnection(TConnection connection)
        {
            connection.Resolve();
        }

        public void ConnectTo(Socket socket) => ConnectTo((TConnection)Activator.CreateInstance(typeof(TConnection), socket));
        
        public void ConnectTo(IPAddress address, int port) => ConnectTo(new IPEndPoint(address, port));

        public void ConnectTo(IPEndPoint endPoint) => ConnectTo((TConnection)Activator.CreateInstance(typeof(TConnection), endPoint));

        public void ConnectTo(TConnection connection)
        {
            if (Connections.Contains(connection))
            {
                return;
            }

            var eventArgs = new ConnectionEventArgs<TConnection, TTxtRecords>(connection);
            var shouldConnect = ShouldConnectToConnection?.Invoke(this, eventArgs) ?? true;

            if (!shouldConnect)
            {
                connection.HandleException(new Exception("The connection request was rejected"));
                return;
            }

            SetupConnection(connection);

            connection.Connect();
        }

        private void SetupConnection(TConnection connection)
        {
            connection.DidUpdateState += baseConnection =>
            {
                var delegateConnection = (TConnection) baseConnection;
                if (Connections.Contains(delegateConnection) && (delegateConnection.State == ConnectionState.Connected || connection.State == ConnectionState.Connecting))
                {
                    return;
                }

                if (delegateConnection.State == ConnectionState.Connected)
                {
                    Connections.Add(delegateConnection);
                }
                else if (delegateConnection.State == ConnectionState.Disconnected && Connections.Contains(delegateConnection))
                {
                    Connections.Remove(delegateConnection);
                }

                DidUpdateConnectionState?.Invoke(this, new ConnectionEventArgs<TConnection, TTxtRecords>(delegateConnection));
            };

            connection.DidUpdateTxtRecords += delegateConnection =>
            {
                var eventArgs = new ConnectionEventArgs<TConnection, TTxtRecords>((TConnection) delegateConnection);
                DidUpdateTxtRecords?.Invoke(this, eventArgs);
            };

            connection.ReceivingUpdatePercentage += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs<TConnection, TTxtRecords>((TConnection) delegateConnection, dataArgs.Data,
                    dataArgs.DataComponent, dataArgs.ActionState, dataArgs.Progress);
                return NormalizedUpdatePercentage(DelegateReceivingUpdatePercentage?.Invoke(this, eventArgs), 5);
            };

            connection.DidUpdateReceivingData += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs<TConnection, TTxtRecords>((TConnection) delegateConnection, dataArgs.Data,
                    dataArgs.DataComponent, dataArgs.ActionState, dataArgs.Progress);
                DidUpdateReceivingData?.Invoke(this, eventArgs);
            };

            connection.SendingUpdatePercentage += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs<TConnection, TTxtRecords>((TConnection) delegateConnection, dataArgs.Data,
                    dataArgs.DataComponent, dataArgs.ActionState, dataArgs.Progress);
                return NormalizedUpdatePercentage(DelegateSendingUpdatePercentage?.Invoke(this, eventArgs), 5);
            };

            connection.DidUpdateSendingData += (delegateConnection, dataArgs) =>
            {
                var eventArgs = new ConnectionEventArgs<TConnection, TTxtRecords>((TConnection) delegateConnection, dataArgs.Data,
                    dataArgs.DataComponent, dataArgs.ActionState, dataArgs.Progress);
                DidUpdateSendingData?.Invoke(this, eventArgs);
            };
        }

        private static int NormalizedUpdatePercentage(int? currentValue, int defaultValue) => Math.Max(1, Math.Min(currentValue ?? defaultValue, 100));

        public void SendData(Data data, TConnection connection)
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

        public void SendString(string stringToSend, TConnection connection) => SendString(stringToSend, Encoding.ASCII, connection);

        public void SendString(string stringToSend, Encoding encoding, TConnection connection) => SendData(new Data(stringToSend, encoding), connection);

        public void SendImage(Image image, TConnection connection) => SendImage(image, "Untitled", connection);

        public void SendImage(Image image, string name, TConnection connection) => SendData(new Data(image, name), connection);

        public void SendFile(string filePath, TConnection connection) => SendFile(filePath, Path.GetFileName(filePath), connection);

        public void SendFile(string filePath, string name, TConnection connection) => SendData(new Data(filePath, name), connection);

        public void SendEncodedString(string encodedString, EncodedDataType encodedDataType, TConnection connection)
        {
            DataType dataType;
            switch (encodedDataType)
            {
                case EncodedDataType.Xml:
                    dataType = DataType.XmlString;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapString;
                    break;
                default:
                    dataType = DataType.JsonString;
                    break;
            }

            SendData(new Data(encodedString, dataType), connection);
        }

        public void SendEncodedObject(object encodedObject, EncodedDataType encodedDataType, TConnection connection)
        {
            var dataType = DataType.JsonObject;
            switch (encodedDataType)
            {
                case EncodedDataType.Xml:
                    dataType = DataType.XmlObject;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapObject;
                    break;
                case EncodedDataType.Binary:
                    dataType = DataType.BinaryObject;
                    break;
            }

            SendData(new Data(encodedObject, dataType), connection);
        }

        public void SendDictionary(IDictionary dictionary, EncodedDataType encodedDataType, TConnection connection)
        {
            DataType dataType;
            switch (encodedDataType)
            {
                case EncodedDataType.Xml:
                    dataType = DataType.XmlDictionary;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapDictionary;
                    break;
                case EncodedDataType.Binary:
                    dataType = DataType.BinaryDictionary;
                    break;
                default:
                    dataType = DataType.JsonDictionary;
                    break;
            }

            SendData(new Data(dictionary, dataType), connection);
        }

        public void SendArray(IList array, EncodedDataType encodedDataType, TConnection connection)
        {
            DataType dataType;
            switch (encodedDataType)
            {
                case EncodedDataType.Xml:
                    dataType = DataType.XmlArray;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapArray;
                    break;
                case EncodedDataType.Binary:
                    dataType = DataType.BinaryArray;
                    break;
                default:
                    dataType = DataType.JsonArray;
                    break;
            }

            SendData(new Data(array, dataType), connection);
        }

        public void SendData(byte[] dataToSend, TConnection connection) => SendData(new Data(dataToSend), connection);

    }
}