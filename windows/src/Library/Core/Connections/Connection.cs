using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Communicate
{
    public class Connection : IEquatable<Connection>
    {
        protected internal Connection(Socket socket)
        {
            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }
            ConnectionSocket = socket;
            Information = new ConnectionInformation((IPEndPoint)socket.RemoteEndPoint);
        }

        protected internal Connection(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }
            Information = new ConnectionInformation(endPoint);
        }

        protected internal Connection()
        {
        }

        public ConnectionState State { get; private set; } = ConnectionState.NotConnected;
        public CommunicatorException ConnectionException { get; private set; }

        public ConnectionInformation Information { get; private set; } = new ConnectionInformation();

        protected Socket ConnectionSocket { get; private set; }
        private Thread BackgroundThread { get; set; }

        internal byte[] TxtRecordsData { get; private set; }
        public Collection<TxtRecord> TxtRecords { get; internal set; }
    
        public virtual bool Equals(Connection other)
        {
            var myEndPoint = Information?.EndPoint;
            var otherEndPoint = other?.Information?.EndPoint;
            if (myEndPoint != null && otherEndPoint != null)
            {
                return myEndPoint.Address.Equals(otherEndPoint.Address);
            }
            return false;
        }

        public int ReceivingUpdatePercentage { get; private set; } = 5;
        public int SendingUpdatePercentage { get; private set; } = 5;

        public void SetReceivingUpdatePercentage(int receivingUpdatePercentage)
        {
            if (receivingUpdatePercentage <= 1 || receivingUpdatePercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(receivingUpdatePercentage), receivingUpdatePercentage, "The value for this property must be between 1 and 100");
            }
            ReceivingUpdatePercentage = receivingUpdatePercentage;
        }

        public void SetSendingUpdatePercentage(int sendingUpdatePercentage)
        {
            if (sendingUpdatePercentage <= 1 || sendingUpdatePercentage > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(sendingUpdatePercentage), sendingUpdatePercentage, "The value for this property must be between 1 and 100");
            }
            SendingUpdatePercentage = sendingUpdatePercentage;
        }

        protected internal event EventHandler DidUpdateState;
        protected internal event EventHandler DidUpdateTxtRecords;
        protected internal event EventHandler DidUpdateInformation;

        protected internal event EventHandler<ConnectionDataEventArgs> DidUpdateReceivingData;

        protected internal event EventHandler<ConnectionDataEventArgs> DidUpdateSendingData;

        protected void UpdateState(ConnectionState newState)
        {
            if (State == newState)
            {
                return;
            }
            State = newState;
            DidUpdateState?.Invoke(this, EventArgs.Empty);
        }

        protected void SetName(string name)
        {
            Information.Name = name;
        }

        protected internal void HandleException(CommunicatorErrorCode errorCode, Exception innerException)
        {
            ConnectionException = new CommunicatorException(errorCode, innerException);
            UpdateState(ConnectionState.Error);
        }

        protected virtual void HandleResolve(Action<IPEndPoint> completion)
        {
            throw new NotImplementedException();
        }

        internal void Resolve()
        {
            UpdateState(ConnectionState.Resolving);
            UpdateTxtRecords();
            HandleResolve(endPoint =>
            {
                if (State != ConnectionState.Resolving)
                {
                    return;
                }

                Information.SetEndPoint(endPoint);
                UpdateState(ConnectionState.Resolved);

                ConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ConnectionSocket.Connect(Information.EndPoint);
                ConnectToSocket(ConnectionSocket);
            });
        }

        protected virtual void HandleUpdateTxtRecords()
        {
            throw new NotImplementedException();
        }
            
        internal void UpdateTxtRecords()
        {
            HandleUpdateTxtRecords();
        }

        internal void Connect()
        {
            if (State == ConnectionState.Connecting || State == ConnectionState.Connected)
            {
                return;
            }
            UpdateState(ConnectionState.Connecting);

            try
            {
                if (!Information.Resolved)
                {
                    Resolve();
                }
                else
                {
                    if (ConnectionSocket == null)
                    {
                        ConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        ConnectionSocket.Connect(Information.EndPoint);
                    }

                    ConnectToSocket(ConnectionSocket);
                }
            }
            catch (ObjectDisposedException exception)
            {
                HandleException(CommunicatorErrorCode.ConnectionClosed, exception);
            }
            catch (SocketException exception)
            {
                HandleException(CommunicatorErrorCode.ConnectionSocketCreationError, exception);
            }
        }

        protected void ConnectToSocket(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                ConnectionSocket = socket;
                Information.SetEndPoint((IPEndPoint)ConnectionSocket.RemoteEndPoint);

                BackgroundThread = new Thread(Receive) { IsBackground = true };
                BackgroundThread.Start();

                UpdateState(ConnectionState.Connected);
            }
            else
            {
                HandleException(CommunicatorErrorCode.ConnectionUnknownError, null);
            }
        }

        protected void SetTxtRecordsData(byte[] txtRecordsData)
        {
            TxtRecordsData = txtRecordsData;
            DidUpdateTxtRecords?.Invoke(this, EventArgs.Empty);
        }

        private static int GenerateUpdateFrequency(int updatePercentage, int numberOfBytes)
        {
            var updateBytesFrequency = numberOfBytes/(100/updatePercentage);
            if (updateBytesFrequency == 0)
            {
                updateBytesFrequency = numberOfBytes;
            }
            return updateBytesFrequency;
        }

        public void Disconnect(bool disconnectImmediately)
        {
            if (!disconnectImmediately)
            {
                Send(new CommunicationData(DataType.Termination));
                return;
            }
            ConnectionSocket.Close();
            ConnectionSocket = null;
            UpdateState(ConnectionState.Disconnected);
        }

        private void Receive()
        {
            while (ConnectionSocket?.Connected ?? false)
            {
                try
                {
                    ReceiveData();
                }
                catch (SocketException)
                {
                    if (ConnectionSocket != null && !ConnectionSocket.Connected)
                    {
                        Disconnect(true);
                    }
                }
            }
            BackgroundThread.Abort();
        }

        private void ReceiveSocketData(int numberOfBytes, int updatePercentage, Action<byte[], float, bool> callback)
        {
            var updateBytesFrequency = GenerateUpdateFrequency(updatePercentage, numberOfBytes);

            if (numberOfBytes == 0)
            {
                callback?.Invoke(new byte[0], 1, true);
                return;
            }

            var data = new byte[numberOfBytes];
            var bytesRead = 0;
            while (bytesRead < data.Length)
            {
                var maxLengthToRead = Math.Min(data.Length - bytesRead, updateBytesFrequency);

                var read = ConnectionSocket.Receive(data, bytesRead, maxLengthToRead, SocketFlags.None);
                if (read <= 0)
                {
                    continue;
                }
                bytesRead += read;

                var progress = (float) bytesRead/numberOfBytes;
                var completed = progress >= 1;
                callback?.Invoke(data, progress, completed);
            }
        }

        private void ReceiveData()
        {
            ReceiveSocketData(DataInfo.DataInfoSize, 100, (bytes, progress, completed) =>
            {
                var dataInfo = new DataInfo(bytes);
                var data = new CommunicationData(dataInfo);

                DidUpdateReceivingData?.Invoke(this,
                    new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Started, 0));

                ReceiveDataHeader(data);
                ReceiveDataContent(data);
                ReceiveDataFooter(data);

                if (data.DataType == DataType.Termination)
                {
                    Disconnect(true);
                    return;
                }
                else if (data.DataType == DataType.ConnectionInformation)
                {
                    Information = (ConnectionInformation)Serialization.InformationSerializer.FromData(data.GetData());
                    DidUpdateInformation(this, EventArgs.Empty);
                    return;
                }

                DidUpdateReceivingData?.Invoke(this,
                    new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Completed, 1));
            });
        }

        private void ReceiveDataComponent(CommunicationData data, int length, DataComponent dataComponent, Action<byte[]> completion)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            DidUpdateReceivingData?.Invoke(this,
                new ConnectionDataEventArgs(data, dataComponent, ActionState.Started, 0));

            ReceiveSocketData(length, ReceivingUpdatePercentage, (bytes, progress, completed) =>
            {
                if (!completed)
                {
                    DidUpdateReceivingData?.Invoke(this,
                        new ConnectionDataEventArgs(data, dataComponent, ActionState.Updating, progress));
                }
                else
                {
                    completion?.Invoke(bytes);
                    DidUpdateReceivingData?.Invoke(this,
                        new ConnectionDataEventArgs(data, dataComponent, ActionState.Completed, 1));
                }
            });
        }

        private void ReceiveDataHeader(CommunicationData data) => 
            ReceiveDataComponent(data, data.Info.HeaderLength, DataComponent.Header, bytes => data.Header = new DataHeaderFooter(bytes));

        private void ReceiveDataContent(CommunicationData data) =>
            ReceiveDataComponent(data, data.Info.ContentLength, DataComponent.Content, bytes => data.InternalContent = bytes);

        private void ReceiveDataFooter(CommunicationData data) =>
            ReceiveDataComponent(data, data.Info.FooterLength, DataComponent.Footer, bytes => data.Footer = new DataHeaderFooter(bytes));

        public void SendInformation()
        {
            var data = Serialization.InformationSerializer.ToData(Information);
            SendData(new CommunicationData().WithContent(data, DataType.ConnectionInformation));
       }

        public void Send(CommunicationData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (!ConnectionSocket.Connected)
            {
                Disconnect(true);
                return;
            }
            
            ThreadPool.QueueUserWorkItem(state => SendData(data));
        }

        private void SendSocketData(byte[] data, int updatePercentage, Action<float, bool> callback = null)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            var updateFrequency = GenerateUpdateFrequency(updatePercentage, data.Length);

            var bytesSent = 0;
            while (bytesSent < data.Length)
            {
                var maxPacketSize = Math.Min(data.Length - bytesSent, updateFrequency);
                var dataSubset = data.Skip(bytesSent).Take(maxPacketSize).ToArray();

                var sent = ConnectionSocket.Send(dataSubset);
                if (sent <= 0)
                {
                    continue;
                }
                bytesSent += sent;

                var progress = (float) bytesSent/data.Length;
                var completed = progress >= 1;
                callback?.Invoke(progress, completed);
            }
        }

        private void SendData(CommunicationData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            data.PrepareForSending();

            DidUpdateSendingData?.Invoke(this,
                new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Started, 0));

            SendSocketData(data.Info?.GetData(), 100);

            SendDataComponent(data, DataComponent.Header, data.Header?.GetData());
            SendDataComponent(data, DataComponent.Content, data.GetData());
            SendDataComponent(data, DataComponent.Footer, data.Footer?.GetData());

            if (data.Info?.DataType == DataType.Termination)
            {
                Disconnect(true);
                return;
            }
            else if (data.Info?.DataType == DataType.ConnectionInformation)
            {
                return;
            }
            DidUpdateSendingData?.Invoke(this,
                new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Completed, 1));
        }

        private void SendDataComponent(CommunicationData data, DataComponent dataComponent, byte[] bytes)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            DidUpdateSendingData?.Invoke(this, new ConnectionDataEventArgs(data, dataComponent, ActionState.Started, 0));
            
            SendSocketData(bytes, SendingUpdatePercentage, (progress, completed) =>
            {
                if (!completed)
                {
                    DidUpdateSendingData?.Invoke(this,
                        new ConnectionDataEventArgs(data, dataComponent, ActionState.Updating, progress));
                }
                else
                {
                    DidUpdateSendingData?.Invoke(this,
                        new ConnectionDataEventArgs(data, dataComponent, ActionState.Completed, 1));
                }
            });
        }
    }
}