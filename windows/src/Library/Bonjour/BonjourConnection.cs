using System;
using System.Net;
using System.Net.Sockets;
using ZeroconfService;

namespace Communicate.Bonjour
{
    public class BonjourConnection : Connection
    {
        public BonjourConnection(Socket socket) : base(socket)
        {
        }

        public BonjourConnection(IPEndPoint endPoint) : base(endPoint)
        {

        }

        public BonjourConnection()
        {
        }

        protected internal BonjourConnection(NetService service): base()
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            ConnectionService = service;
            SetName(service.Name);
        }

        private NetService ConnectionService { get; }

        protected override void HandleResolve(Action<IPEndPoint> completion)
        {
            ConnectionService.DidResolveService += service =>
            {
                ConnectionService.Stop();
                completion?.Invoke((IPEndPoint)service.Addresses[0]);
            };

            ConnectionService.DidNotResolveService += (service, exception) =>
            {
                var errorCode = CommunicatorErrorCode.ResolvingUnknownError;
                if (exception.ErrorType == DNSServiceErrorType.Timeout)
                {
                    errorCode = CommunicatorErrorCode.ResolvingTimedOut;
                }
                HandleException(errorCode, exception);

            };
            ConnectionService.ResolveWithTimeout(10);
        }

        protected override void HandleUpdateTxtRecords()
        {
            ConnectionService.DidUpdateTXT += service =>
            {
                SetTxtRecords(new BonjourTxtRecordCollection(service.TXTRecordData));
            };
            ConnectionService.StartMonitoring();
        }

        public bool Equals(BonjourConnection connection)
        {
            var myService = ConnectionService;
            var otherService = connection?.ConnectionService;
            if (myService != null && otherService != null)
            {
                return myService.Name == otherService.Name &&
                       myService.Port == otherService.Port;
            }
            return base.Equals(connection);
        }
    }
}