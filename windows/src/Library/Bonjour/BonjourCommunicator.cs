using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ZeroconfService;

namespace Communicate.Bonjour
{
    public class BonjourCommunicator : BaseCommunicator
    {
        public BonjourCommunicator(CommunicatorInformation communicatorInformation, CommunicatorProtocol protocol) : base(communicatorInformation, protocol)
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
                PublishedService = new NetService(null, SerializeProtocolType(), Information.Name, Information.Port);
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

            PublishedService.TXTRecordData = DataFromTxtRecords(TxtRecords);

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


        public override Collection<TxtRecord> TxtRecordsFromData(byte[] data)
        {
            var txtRecords = new Collection<TxtRecord>();

            var dictionary = NetService.DictionaryFromTXTRecordData(data);
            foreach (DictionaryEntry entry in dictionary)
            {
                var key = entry.Key as string;
                var value = entry.Value as byte[];
                if (key!= null && value != null)
                {
                    var txtRecord = new TxtRecord(key, Encoding.ASCII.GetString(value));
                    txtRecords.Add(txtRecord);
                }
            }

            return txtRecords;
        }

        public override byte[] DataFromTxtRecords(Collection<TxtRecord> txtRecords)
        {
            var records = txtRecords.ToDictionary(record => record.Key, record => record.Value);
            return NetService.DataFromTXTRecordDictionary(records);
        }

        public override string SerializeProtocolType() => "_" + Protocol.Name + "._" + Protocol.TransportString;
    }
}