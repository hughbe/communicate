using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroconfService;
using Communicate.Common;

namespace Communicate.Publishing
{
    internal delegate void StartedPublishing(PublishingManager publishingManager);
    internal delegate void Published(PublishingManager publishingManager);
    internal delegate void NotPublished(PublishingManager publishingManager, Exception reason);
    internal delegate void Unpublished(PublishingManager publishingManager);

    public class PublishingManager : IDisposable
    {
        #region Private Variables

        private ProtocolInfo _protocol;
        private CommunicatorInfo _communicatorInfo;

        private NetService _publishedService;
        private PublishingState _publishingState;

        #endregion

        #region Properties

        /// <summary>
        /// The currently published service
        /// </summary>
        public NetService PublishedService
        {
            get { return _publishedService; }
        }

        /// <summary>
        /// A value that indicates the publishing state of the publishing manager
        /// </summary>
        public PublishingState PublishingState
        {
            get { return _publishingState; }
        }

        /// <summary>
        /// The protocol with which to publish the communicator
        /// </summary>
        public ProtocolInfo Protocol
        {
            get { return _protocol; }
        }

        /// <summary>
        /// The information about the communicator to publish
        /// </summary>
        public CommunicatorInfo CommunicatorInfo
        {
            get { return _communicatorInfo; }
        }

        /// <summary>
        /// The event called when the publishing manager starts publishing
        /// </summary>
        internal event StartedPublishing DidStartPublishing;

        /// <summary>
        /// The event called when the publishing manager published successfully
        /// </summary>
        internal event Published DidPublish;

        /// <summary>
        /// The event called when the publishing manager failed to publish
        /// </summary>
        internal event NotPublished DidNotPublish;

        /// <summary>
        /// The event called when the publishing manager unpublishes
        /// </summary>
        internal event Unpublished DidUnpublish;

        #endregion

        #region Starting

        /// <summary>
        /// Constructs a publishing manager from information about the protocol and communicator
        /// </summary>
        /// <param name="protocolInfo">The information about the protocol with which to publish</param>
        /// <param name="communicatorInfo">The information about the communicator to publish</param>
        internal PublishingManager(ProtocolInfo protocolInfo, CommunicatorInfo communicatorInfo)
        {
            if (protocolInfo == null)
            {
                throw new ArgumentNullException("protocolInfo");
            }
            else if (communicatorInfo == null)
            {
                throw new ArgumentNullException("communicatorInfo");
            }
            else
            {
                _protocol = protocolInfo;
                _communicatorInfo = communicatorInfo;
                _publishingState = PublishingState.NotPublished;
            }
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private PublishingManager()
        {

        }

        /// <summary>
        /// Publishes the communicator on the network using the information given about the protocol and the communicator
        /// </summary>
        public void Publish()
        {
            if (_publishingState == PublishingState.Publishing || _publishingState == PublishingState.Published)
            {
                return;
            }

            _publishingState = PublishingState.Publishing;
            try
            {
                if (DidStartPublishing != null)
                {
                    DidStartPublishing(this);
                }

                _publishedService = new NetService(_protocol.Domain, _protocol.SerializeType(false), _communicatorInfo.ReadableName, _communicatorInfo.Port);

                _publishedService.TXTRecordData = _communicatorInfo.TXTRecordList.Serialize();

                _publishedService.DidPublishService += new NetService.ServicePublished(DidPublishService);
                _publishedService.DidNotPublishService += new NetService.ServiceNotPublished(DidNotPublishService);

                _publishedService.Publish();
                if (DidStartPublishing != null)
                {
                    DidStartPublishing(this);
                }
            }
            catch (Exception exception)
            {
                _publishingState = PublishingState.ErrorPublishing;
                if (DidNotPublish != null)
                {
                    DidNotPublish(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when the NetService successfully publishes itself on the network
        /// </summary>
        /// <param name="service">The service that was published on the network</param>
        private void DidPublishService(NetService service)
        {
            _publishingState = PublishingState.Published;
            _publishedService = service;
            if (DidPublish != null)
            {
                DidPublish(this);
            }
        }

        /// <summary>
        /// The delegate method called when the NetService fails to publish itself on the network
        /// </summary>
        /// <param name="service">The service that failed to be published on the network</param>
        /// <param name="exception">The reason why the service failed to publish</param>
        private void DidNotPublishService(NetService service, DNSServiceException exception)
        {
            _publishingState = PublishingState.ErrorPublishing;
            if (DidNotPublish != null)
            {
                DidNotPublish(this, exception);
            }
        }

        #endregion

        #region Ending

        /// <summary>
        /// Unpublishes the communicator on the network
        /// </summary>
        public void Unpublish()
        {
            if (_publishingState == PublishingState.Published || _publishingState == PublishingState.Publishing)
            {
                _publishingState = PublishingState.UnPublished;

                _publishedService.Dispose();
                _publishedService = null;

                if (DidUnpublish != null)
                {
                    DidUnpublish(this);
                }
            }
        }

        /// <summary>
        /// Dipsposes all objects managed by the publishing manager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects managed by the publishing manager
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_publishedService != null) { _publishedService.Dispose(); _publishedService = null; }
            }
        }

        #endregion

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the publishing manager in a readable format</returns>
        public override string ToString()
        {
            return "Publishing Manager: state = " + _publishingState.ToString();
        }
    }
}
