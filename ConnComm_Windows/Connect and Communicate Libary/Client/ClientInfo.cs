using System;

namespace Communicate.Client
{
    /// <summary>
    /// The class responsible for containing the backend information the client that dictates how the client will listen for and handle incoming connection requests and streams of contentData
    /// </summary>
    public class ClientInfo
    {
        private int _port;

        #region Properties

        /// <summary>
        /// The port for the client to listen on for incoming connection requests and contentData streams
        /// </summary>
        public int Port
        {
            get { return _port; }
        }

        #endregion
            
        /// <summary>
        /// The default constructor for the ClientInfo object
        /// </summary>
        /// <param name="port">The port for the client to listen on for incoming connection requests and contentData streams</param>
        public ClientInfo(int port)
        {
            _port = port;
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private ClientInfo()
        {

        }

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the client in a readable format</returns>
        public override string ToString()
        {
            return "ClientInfo: port = " + _port;
        }
    }
}