using Communicate.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroconfService;

namespace Communicate.Searching
{
    internal delegate void StartedSearching(SearchingManager searchingManager);
    internal delegate void NotSearching(SearchingManager searchingManager, Exception reason);
    internal delegate void FoundServices(SearchingManager searchingManager, List<NetService> services);
    internal delegate void StoppedSearching(SearchingManager searchingManager);

    public class SearchingManager : IDisposable
    {
        #region Private Variables

        private SearchingState _searchingState;

        private ProtocolInfo _protocol;

        private NetServiceBrowser _browser;
        private List<NetService> _services;

        #endregion

        #region Properties

        /// <summary>
        /// A value that indicates the searching state of the searching manager
        /// </summary>
        public SearchingState SearchingState 
        {
            get { return _searchingState; }
        }
        /// <summary>
        /// The protocol that the searching manager uses to search for devices on the network publihsing themselves under this protocol
        /// </summary>
        public ProtocolInfo Protocol 
        {
            get { return _protocol; }
        }

        /// <summary>
        /// The Bonjour browser that searches for devices on the network publishing themselves using the protocol with which the searching manager was constructed
        /// </summary>
        public NetServiceBrowser Browser 
        {
            get { return _browser; }
        }

        /// <summary>
        /// The list of Bonjour services discovered on the network
        /// </summary>
        public List<NetService> Services
        {
            get { return _services; }
        }

        /// <summary>
        /// The event called when the searching manager starts searching for devices on the network
        /// </summary>
        internal event StartedSearching DidStartSearching;

        /// <summary>
        /// The event called when the searching manager finds devices on the network
        /// </summary>
        internal event FoundServices DidFindServices;

        /// <summary>
        /// The event called when the searching manager fails to search for devices on the network
        /// </summary>
        internal event NotSearching DidNotSearch;

        /// <summary>
        /// The event called when the searching manager stops searching for devices on the network
        /// </summary>
        internal event StoppedSearching DidStopSearching;

        #endregion

        #region Starting

        /// <summary>
        /// Constructs a searching manager from information about the protocol with which to search for devices on the network
        /// </summary>
        /// <param name="protocol">The information about the protocol with which to search for devices on the network</param>
        internal SearchingManager(ProtocolInfo protocol) 
        {
            if(protocol == null) 
            {
                throw new ArgumentNullException("protocol");
            }
            else
            {
                _protocol = protocol;

                _searchingState = SearchingState.NotSearching;
                _services = new List<NetService>();

                _browser = new NetServiceBrowser();
                _browser.DidFindService += netServiceBrowser_DidFindService;
                _browser.DidRemoveService += netServiceBrowser_DidRemoveService;
            }
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private SearchingManager() 
        {

        }

        /// <summary>
        /// Starts searching for devices on the network publishing themselves using the protocol with which the searching manager was constructed
        /// </summary>
        public void StartSearching()
        {
            if (_searchingState == SearchingState.Searching)
            {
                return;
            }

            _searchingState = SearchingState.Searching;
            if (DidStartSearching != null)
            {
                DidStartSearching(this);
            }
            try
            {
                _browser.SearchForService(_protocol.SerializeType(false), _protocol.Domain);
            }
            catch (Exception exception)
            {
                _searchingState = SearchingState.ErrorSearching;
                if (DidNotSearch != null)
                {
                    DidNotSearch(this, exception);
                }
            }
        }

        /// <summary>
        /// The delegate method called when a Bonjour browser removes a service that was previously published on the network
        /// </summary>
        /// <param name="browser">The Bonjour browser that removed the Bonjour service previously published on the network</param>
        /// <param name="service">The Bonjour service that was previously published on the network but no longer is published</param>
        /// <param name="moreComing">If there are more previously published Bonjour services that are to be removed</param>
        private void netServiceBrowser_DidRemoveService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            if (_services.Contains(service))
            {
                _services.Remove(service);
            }
            if (!moreComing)
            {
                if (DidFindServices != null)
                {
                    DidFindServices(this, _services);
                }
            }
        }

        /// <summary>
        /// The delegate method called when a Bonjour browser finds a device published on the network
        /// </summary>
        /// <param name="browser">The Bonjour browser that found the Bonjour service published on the network</param>
        /// <param name="service">The Bonjour service that was published on the network</param>
        /// <param name="moreComing">If there are previously published Bonjour services that are to be found</param>
        private void netServiceBrowser_DidFindService(NetServiceBrowser browser, NetService service, bool moreComing)
        {
            if (!_services.Contains(service))
            {
                bool addService = true;
                foreach(NetService existingService in _services) 
                {
                    if (existingService.Name.Equals(service.Name))
                    {
                        addService = false;
                    }
                }
                if (addService)
                {
                    _services.Add(service);
                }
            }
            if (!moreComing)
            {
                if (DidFindServices != null)
                {
                    DidFindServices(this, _services);
                }
            }
        }

        #endregion

        #region Ending

        /// <summary>
        /// Stops searching for devices on the network
        /// </summary>
        public void StopSearching()
        {
            if (_searchingState == SearchingState.Searching)
            {
                _searchingState = SearchingState.NotSearching;
                _browser.Stop();
                if (DidStopSearching != null)
                {
                    DidStopSearching(this);
                }
            }
        }

        /// <summary>
        /// Dipsposes all objects managed by the searching manager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects managed by the searching manager
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_browser != null) { _browser.Dispose(); _browser = null; }
            }
        }

        #endregion

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the searching manager in a readable format</returns>
        public override string ToString()
        {
            return "Searching Manager: state = " + _searchingState.ToString();
        }

    }
}
