using Communicate.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ZeroconfService;

namespace Communicate.Listening
{
    internal delegate void StartedListening(ListeningManager listeningManager);
    internal delegate void ReceivedConnectionRequest(ListeningManager listeningManager, Socket connectionSocket);
    internal delegate void NotStartedListening(ListeningManager listeningManager, Exception reason);
    internal delegate void StoppedListening(ListeningManager listeningManager);

    public class ListeningManager : IDisposable
    {
        #region Private Variables

        private CommunicatorInfo _communicatorInfo;

        private TcpListener _listener;
        private ListeningState _listeningState;

        #endregion

        #region Properties

        /// <summary>
        /// The information about the communicator for which to listen for connection requests
        /// </summary>
        public CommunicatorInfo CommunicatorInfo
        {
            get { return _communicatorInfo; }
        }

        /// <summary>
        /// A value that indicates the listening state of the listening manager
        /// </summary>
        public ListeningState ListeningState 
        {
            get { return _listeningState; }
        }

        /// <summary>
        /// The event called when the listening manager start listening for incoming connections
        /// </summary>
        internal event StartedListening DidStartListening;

        /// <summary>
        /// The event called when the listening manager receives an incoming connection request
        /// </summary>
        internal event ReceivedConnectionRequest DidReceiveConnectionRequest;

        /// <summary>
        /// The event called when the listening manager fails to start listening for incoming connections
        /// </summary>
        internal event NotStartedListening DidNotStartListening;

        /// <summary>
        /// The event called when the listening manager stops listening for incoming connections
        /// </summary>
        internal event StoppedListening DidStopListening;

        #endregion

        #region Starting

        /// <summary>
        /// Constructs a listening manager from information about the communicator
        /// </summary>
        /// <param name="communicatorInfo">The information about the communicator for which to listen</param>
        internal ListeningManager(CommunicatorInfo communicatorInfo)
        {
            if (communicatorInfo == null)
            {
                throw new ArgumentNullException("communicatorInfo");
            }
            else
            {
                _communicatorInfo = communicatorInfo;
                _listener = new TcpListener(IPAddress.Any, _communicatorInfo.Port);
            }
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private ListeningManager()
        {

        }

        /// <summary>
        /// Starts listening for any incoming connection requests
        /// </summary>
        public void StartListening()
        {
            _listeningState = ListeningState.Listening;
            try
            {
                _listener.Start(10);
                _listener.BeginAcceptSocket(new AsyncCallback(ListenerAcceptSocketCallback), _listener);
                if (DidStartListening != null)
                {
                    DidStartListening(this);
                }
            }
            catch (Exception exception)
            {
                _listeningState = ListeningState.ErrorListening;
                if (DidNotStartListening != null)
                {
                    DidNotStartListening(this, exception);
                }
            }
        }

        /// <summary>
        /// The callback called when the listener receives a connection request
        /// </summary>
        /// <param name="asyncResult">The result of the connection request</param>
        public void ListenerAcceptSocketCallback(IAsyncResult asyncResult)
        {
            TcpListener listener = (TcpListener)asyncResult.AsyncState;
            try
            {
                Socket clientSocket = listener.EndAcceptSocket(asyncResult);
                if (DidReceiveConnectionRequest != null)
                {
                    DidReceiveConnectionRequest(this, clientSocket);
                }
                listener.BeginAcceptSocket(new AsyncCallback(ListenerAcceptSocketCallback), listener);
            }
            catch (Exception exception) { }
        }

        #endregion

        #region Ending

        /// <summary>
        /// Stops listening for incoming connection requests
        /// </summary>
        public void StopListening()
        {
            if (_listeningState == ListeningState.Listening)
            {
                _listeningState = ListeningState.StoppedListening;
                if (_listener != null)
                {
                    _listener.Stop();
                    if (DidStopListening != null)
                    {
                        DidStopListening(this);
                    }
                }
            }
        }

        /// <summary>
        /// Dipsposes all objects managed by the listening manager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dipsposes all objects managed by the listening manager
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_listener != null) { _listener.Stop(); _listener = null; }
            }
        }

        #endregion

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the listening manager in a readable format</returns>
        public override string ToString()
        {
            return "Listening Manager: state = " + _listeningState.ToString();
        }
    }
}
