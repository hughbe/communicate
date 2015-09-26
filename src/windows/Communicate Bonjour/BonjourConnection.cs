using System;
using System.Net;
using System.Net.Sockets;
using ZeroconfService;

namespace Communicate.Bonjour
{
    public class BonjourConnection : BaseConnection<BonjourTxtRecords>
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

        protected internal BonjourConnection(NetService service)
        {
            ConnectionService = service;
            Name = service.Name;
        }

        public NetService ConnectionService { get; }

        protected override void HandleResolve(Action<IPEndPoint> completion)
        {
            ConnectionService.DidResolveService += service =>
            {
                ConnectionService.Stop();
                completion?.Invoke((IPEndPoint)service.Addresses[0]);
            };

            ConnectionService.DidNotResolveService += (service, exception) => HandleException(exception);
            ConnectionService.ResolveWithTimeout(10);
        }

        protected override void HandleUpdateTxtRecords()
        {
            ConnectionService.DidUpdateTXT += service =>
            {
                SetTxtRecords(new BonjourTxtRecords(service.TXTRecordData));
            };
            ConnectionService.StartMonitoring();
        }

        public bool Equals(BonjourConnection connection)
        {
            if (connection.ConnectionService != null && ConnectionService != null)
            {
                return connection.ConnectionService.Name == ConnectionService.Name &&
                       connection.ConnectionService.Port == ConnectionService.Port;
            }
            return base.Equals(connection);
        }
    }
}