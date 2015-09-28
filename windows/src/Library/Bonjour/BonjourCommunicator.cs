using System;
using ZeroconfService;

namespace Communicate.Bonjour
{
    public class BonjourCommunicator : BaseCommunicator
    {
        public BonjourCommunicator(CommunicatorInformation communicatorInformation, Protocol protocol) : base(communicatorInformation, protocol)
        {
            try
            {
                DevicesBrowser = new NetServiceBrowser();
            }
            catch (DNSServiceException exception)
            {
                var errorCode = CommunicatorErrorCode.SearchingUnknownError;
                if (exception.ErrorType == DNSServiceErrorType.Unsupported)
                {
                    errorCode = CommunicatorErrorCode.SearchingNotSupported;
                }
                HandleSearchingException(errorCode, exception);
            }
        }

        public new BonjourTxtRecordCollection TxtRecords { get; set; }

        private NetService PublishedService { get; set; }
        private NetServiceBrowser DevicesBrowser { get; set; }
        
        protected override void Dispose(bool disposing)
        {
            DevicesBrowser?.Dispose();
            DevicesBrowser = null;
            PublishedService?.Dispose();
            PublishedService = null;
            base.Dispose(disposing);
        }

        protected override void HandlePublish()
        {
            try
            {
                PublishedService = new NetService(null, SerializeProtocolType(),
                           CommunicatorInformation.Name, CommunicatorInformation.Port);
            }
            catch (DNSServiceException exception)
            {
                PublishedService?.Dispose();
                var errorCode = CommunicatorErrorCode.PublishingUnknownError;
                if (exception.ErrorType == DNSServiceErrorType.Unsupported)
                {
                    errorCode = CommunicatorErrorCode.PublishingNotSupported;
                }
                HandlePublishingException(errorCode, exception);
                return;
            }

            PublishedService.TXTRecordData = TxtRecords?.Serialize();

            PublishedService.DidPublishService += netService =>
            {
                PublishedService = netService;
                UpdatePublishedState(State.Started);
            };

            PublishedService.DidNotPublishService += (netService, exception) =>
            {
                var errorCode = CommunicatorErrorCode.PublishingUnknownError;
                if (exception.ErrorType == DNSServiceErrorType.Unsupported)
                {
                    errorCode = CommunicatorErrorCode.PublishingNotSupported;
                }
                else if (exception.ErrorType == DNSServiceErrorType.Timeout)
                {
                    errorCode = CommunicatorErrorCode.PublishingTimedOut;
                }
                else if (exception.ErrorType == DNSServiceErrorType.AlreadyRegistered)
                {
                    errorCode = CommunicatorErrorCode.PublishingAlreadyRegistered;
                }
                else if (exception.ErrorType == DNSServiceErrorType.NameConflict)
                {
                    errorCode = CommunicatorErrorCode.PublishingNamingCollision;
                }
                else if (exception.ErrorType == DNSServiceErrorType.Firewall)
                {
                    errorCode = CommunicatorErrorCode.PublishingFirewallBlocked;
                }
                HandlePublishingException(errorCode, exception);
            };

            PublishedService.Publish();
        }

        protected override void HandleStopPublishing()
        {
            PublishedService?.Dispose();
            PublishedService = null;
        }

        protected override void HandleStartSearching()
        {
            try
            {
                DevicesBrowser.DidFindService += (browser, service, moreComing) => AddService(new BonjourConnection(service));
                DevicesBrowser.DidRemoveService += (browser, service, moreComing) => RemoveService(new BonjourConnection(service));
                DevicesBrowser.SearchForService(SerializeProtocolType(), null);
            }
            catch (DNSServiceException exception)
            {
                var errorCode = CommunicatorErrorCode.SearchingUnknownError;
                if (exception.ErrorType == DNSServiceErrorType.Unsupported)
                {
                    errorCode = CommunicatorErrorCode.SearchingNotSupported;
                }
                else if (exception.ErrorType == DNSServiceErrorType.Timeout)
                {
                    errorCode = CommunicatorErrorCode.SearchingTimedOut;
                }
                else if (exception.ErrorType == DNSServiceErrorType.Firewall)
                {
                    errorCode = CommunicatorErrorCode.SearchingFirewallBlocked;
                }
                HandleSearchingException(errorCode, exception);
            }
        }

        protected override void HandleStopSearching()
        {
            DevicesBrowser.Stop();
        }

        public override string SerializeProtocolType() => "_" + Protocol.Name + "._" + Protocol.TransportString;
    }
}