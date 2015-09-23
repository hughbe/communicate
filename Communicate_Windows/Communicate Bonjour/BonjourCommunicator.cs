using ZeroconfService;

namespace Communicate.Bonjour
{
    public class BonjourCommunicator : PublishingCommunicator<BonjourProtocol, BonjourTxtRecords, BonjourConnection>
    {
        public BonjourCommunicator(CommunicatorInformation communicatorInformation, BonjourProtocol protocol) : base(communicatorInformation, protocol)
        {
        }

        private NetService PublishedService { get; set; }
        private NetServiceBrowser DevicesBrowser { get; set; } = new NetServiceBrowser();

        protected override void Dispose(bool disposing)
        {
            if (DevicesBrowser != null)
            {
                DevicesBrowser.Dispose();
                DevicesBrowser = null;
            }
            if (PublishedService != null)
            {
                PublishedService.Stop();
                PublishedService = null;
            }
            base.Dispose(disposing);
        }

        protected override void HandlePublish()
        {
            PublishedService = new NetService(Protocol.Domain, Protocol.SerializeType(),
                       CommunicatorInformation.Name, CommunicatorInformation.Port)
            { TXTRecordData = TxtRecords.Serialize() };

            PublishedService.DidPublishService += netService =>
            {
                PublishedService = netService;
                UpdatePublishedState(State.Started);
            };

            PublishedService.DidNotPublishService += (netService, exception) => HandlePublishingException(exception);

            PublishedService.Publish();
        }

        protected override void HandleUnpublish()
        {
            PublishedService.Dispose();
            PublishedService = null;
        }

        protected override void HandleStartSearching()
        {
            DevicesBrowser.DidFindService += (browser, service, moreComing) => AddService(new BonjourConnection(service));
            DevicesBrowser.DidRemoveService += (browser, service, moreComing) => RemoveService(new BonjourConnection(service));
            DevicesBrowser.SearchForService(Protocol.SerializeType(), Protocol.Domain);
        }

        protected override void HandleStopSearching()
        {
            DevicesBrowser.Stop();
        }
    }
}