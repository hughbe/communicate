using System;
using System.Collections.Generic;
using System.Linq;

namespace Communicate
{
    public abstract class PublishingCommunicator<TProtocol, TTxtRecords, TConnection>: BaseCommunicator<TProtocol, TTxtRecords, TConnection> where TProtocol : BaseProtocol where TConnection : BaseConnection<TTxtRecords>, new() where TTxtRecords : BaseTxtRecords
    {
        protected PublishingCommunicator(CommunicatorInformation communicatorInformation, TProtocol protocol) : base(communicatorInformation, protocol)
        {
        }

        public State PublishedState { get; private set; } = State.Ready;
        public Exception PublishingException { get; private set; }

        public State SearchingState { get; private set; } = State.Ready;
        public Exception SearchingException { get; private set; }

        public List<TConnection> DiscoveredServices { get; } = new List<TConnection>();

        public event UpdatedState DidUpdatePublishedState;

        public event UpdatedState DidUpdateSearchingState;
        public event UpdatedState DidUpdateServices;

        public override void Stop()
        {
            UnpublishFromNetwork();
            StopSearchingForDevices();
            base.Stop();
        }

        protected void UpdatePublishedState(State newState)
        {
            if (newState == PublishedState)
            {
                return;
            }
            PublishedState = newState;
            DidUpdatePublishedState?.Invoke(this);
        }

        protected void HandlePublishingException(Exception exception)
        {
            PublishingException = exception;
            UpdatePublishedState(State.Error);
        }

        protected abstract void HandlePublish();
        protected abstract void HandleUnpublish();

        public void PublishOnNetwork()
        {
            if (PublishedState == State.Starting || PublishedState == State.Started)
            {
                return;
            }

            UpdatePublishedState(State.Starting);

            try
            {
                HandlePublish();
            }
            catch (Exception exception)
            {
                HandlePublishingException(exception);
            }
        }

        public void UnpublishFromNetwork()
        {
            if (PublishedState != State.Starting && PublishedState != State.Started)
            {
                return;
            }

            HandleUnpublish();

            UpdatePublishedState(State.Stopped);
        }

        public void RepublishOnNetwork()
        {
            UnpublishFromNetwork();
            PublishOnNetwork();
        }

        protected void UpdateSearchingState(State newState)
        {
            if (newState == SearchingState)
            {
                return;
            }
            SearchingState = newState;
            DidUpdateSearchingState?.Invoke(this);
        }

        protected void HandleSearchingException(Exception exception)
        {
            SearchingException = exception;
            UpdateSearchingState(State.Error);
        }

        protected abstract void HandleStartSearching();
        protected abstract void HandleStopSearching();

        protected void AddService(TConnection service)
        {
            if (DiscoveredServices.Contains(service) || DiscoveredServices.Count(aService => aService.Name.Equals(service.Name)) != 0)
            {
                return;
            }
            DiscoveredServices.Add(service);
            DidUpdateServices?.Invoke(this);
        }

        protected void RemoveService(TConnection service)
        {
            if (!DiscoveredServices.Contains(service))
            {
                return;
            }
            DiscoveredServices.Remove(service);
            DidUpdateServices?.Invoke(this);
        }

        public void StartSearchingForDevices()
        {
            if (SearchingState == State.Started)
            {
                return;
            }

            try
            {
                HandleStartSearching();
                UpdateSearchingState(State.Started);
            }
            catch (Exception exception)
            {
                HandleSearchingException(exception);
            }
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
    }
}
