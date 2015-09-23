using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Communicate
{
    public abstract class BaseConnection<TTxtRecords> : IEquatable<BaseConnection<TTxtRecords>> where TTxtRecords : BaseTxtRecords
    {
        protected BaseConnection(Socket socket) : this((IPEndPoint)socket.RemoteEndPoint)
        {
            ConnectionSocket = socket;
        }

        protected BaseConnection(IPEndPoint endPoint)
        {
            RemoteEndPoint = endPoint;
            Resolved = true;
        }

        protected BaseConnection()
        {
        }

        public ConnectionState State { get; private set; } = ConnectionState.NotConnected;
        public Exception ConnectionException { get; protected set; }

        public bool Resolved { get; private set; }
        public IPEndPoint RemoteEndPoint { get; protected set; }
        public string Name { get; protected set; }

        protected Socket ConnectionSocket { get; set; }
        private Thread BackgroundThread { get; set; }
        
        public TTxtRecords TxtRecords { get; private set; }
    
        public virtual bool Equals(BaseConnection<TTxtRecords> connection)
        {
            if (connection.RemoteEndPoint != null && RemoteEndPoint != null)
            {
                return connection.RemoteEndPoint.Address.Equals(RemoteEndPoint.Address);
            }
            return false;
        }

        protected internal delegate void ConnectionDelegate(object sender);
        protected internal delegate int? ConnectionUpdatePercentageDelegate(object sender, ConnectionDataEventArgs e);
        protected internal delegate void ConnectionUpdateDataDelegate(object sender, ConnectionDataEventArgs e);

        protected internal event ConnectionDelegate DidUpdateState;
        protected internal event ConnectionDelegate DidUpdateTxtRecords;

        protected internal event ConnectionUpdatePercentageDelegate ReceivingUpdatePercentage;
        protected internal event ConnectionUpdateDataDelegate DidUpdateReceivingData;

        protected internal event ConnectionUpdatePercentageDelegate SendingUpdatePercentage;
        protected internal event ConnectionUpdateDataDelegate DidUpdateSendingData;

        protected void UpdateState(ConnectionState newState)
        {
            if (State == newState)
            {
                return;
            }
            State = newState;
            DidUpdateState?.Invoke(this);
        }

        protected internal void HandleException(Exception exception)
        {
            ConnectionException = exception;
            UpdateState(ConnectionState.Error);
        }

        protected abstract void HandleResolve(Action<IPEndPoint> completion);

        internal void Resolve(Action<Socket> completion = null)
        {
            UpdateState(ConnectionState.Resolving);
            UpdateTxtRecords();
            HandleResolve(endPoint =>
            {
                if (State != ConnectionState.Resolving)
                {
                    return;
                }

                Resolved = true;
                UpdateState(ConnectionState.Resolved);
                RemoteEndPoint = endPoint;

                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(RemoteEndPoint);

                completion?.Invoke(socket);
            });
        }

        protected abstract void HandleUpdateTxtRecords();

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
                Action<Socket> completion = ConnectToSocket;

                if (!Resolved)
                {
                    Resolve(completion);
                }
                else
                {
                    if (ConnectionSocket == null)
                    {
                        ConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        ConnectionSocket.Connect(RemoteEndPoint);
                    }

                    completion(ConnectionSocket);
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        protected void ConnectToSocket(Socket socket)
        {
            if (socket.Connected)
            {
                ConnectionSocket = socket;
                RemoteEndPoint = ConnectionSocket.RemoteEndPoint as IPEndPoint;

                BackgroundThread = new Thread(Receive) { IsBackground = true };
                BackgroundThread.Start();

                UpdateState(ConnectionState.Connected);
            }
            else
            {
                HandleException(new Exception("Error opening socket"));
            }
        }

        protected void SetTxtRecords(TTxtRecords txtRecords)
        {
            TxtRecords = txtRecords;
            DidUpdateTxtRecords?.Invoke(this);
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
                Send(new Data(DataType.Termination));
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
                var data = new Data(dataInfo);

                DidUpdateReceivingData?.Invoke(this,
                    new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Started, 0));

                ReceiveDataHeader(data);
                ReceiveDataContent(data);
                ReceiveDataFooter(data);

                if (data.GetDataType() == DataType.Termination)
                {
                    Disconnect(true);
                    return;
                }

                DidUpdateReceivingData?.Invoke(this,
                    new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Completed, 1));
            });
        }

        private void ReceiveDataComponent(Data data, int length, DataComponent dataComponent, Action<byte[]> completion)
        {
            DidUpdateReceivingData?.Invoke(this,
                new ConnectionDataEventArgs(data, dataComponent, ActionState.Started, 0));

            var updateBytesPercentage =
                ReceivingUpdatePercentage?.Invoke(this,
                    new ConnectionDataEventArgs(data, dataComponent, ActionState.None, 1)) ?? 100;

            ReceiveSocketData(length, updateBytesPercentage, (bytes, progress, completed) =>
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

        private void ReceiveDataHeader(Data data) => 
            ReceiveDataComponent(data, data.Info.HeaderLength, DataComponent.Header, bytes => data.Header = new DataHeaderFooter(bytes));

        private void ReceiveDataContent(Data data) =>
            ReceiveDataComponent(data, data.Info.ContentLength, DataComponent.Content, bytes => data.Content = bytes);

        private void ReceiveDataFooter(Data data) =>
            ReceiveDataComponent(data, data.Info.FooterLength, DataComponent.Footer, bytes => data.Footer = new DataHeaderFooter(bytes));

        public void Send(Data data)
        {
            if (data == null)
            {
                return;
            }

            if (!ConnectionSocket.Connected)
            {
                Disconnect(true);
                return;
            }

            try
            {
                ThreadPool.QueueUserWorkItem(state => SendData(data));
            }
            catch
            {
                if (data.Info?.Type == DataType.Termination || !ConnectionSocket.Connected)
                {
                    Disconnect(true);
                }
            }
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

        private void SendData(Data data)
        {
            DidUpdateSendingData?.Invoke(this,
                new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Started, 0));

            SendSocketData(data.Info?.GetData(), 100);

            SendDataComponent(data, DataComponent.Header, data.Header?.GetData());
            SendDataComponent(data, DataComponent.Content, data.Content);
            SendDataComponent(data, DataComponent.Footer, data.Footer?.GetData());

            if (data.Info?.Type == DataType.Termination)
            {
                Disconnect(true);
                return;
            }
            DidUpdateSendingData?.Invoke(this,
                new ConnectionDataEventArgs(data, DataComponent.All, ActionState.Completed, 1));
        }

        private void SendDataComponent(Data data, DataComponent dataComponent, byte[] bytes)
        {
            DidUpdateSendingData?.Invoke(this, new ConnectionDataEventArgs(data, dataComponent, ActionState.Started, 0));

            var updateBytesPercentage =
                SendingUpdatePercentage?.Invoke(this,
                    new ConnectionDataEventArgs(data, dataComponent, ActionState.None, 0)) ?? 100;

            SendSocketData(bytes, updateBytesPercentage, (progress, completed) =>
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