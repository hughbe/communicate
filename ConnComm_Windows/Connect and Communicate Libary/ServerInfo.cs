using System;

namespace ConnComm
{
    /// <summary>
    /// The class responsible for containing the backend information the server that dictates how the server will listen for and handle incoming connection requests and streams of data
    /// </summary>
    public class ServerInfo
    {
        private string _readableName;
        private int _port;
        private TXTRecordList _TXTRecordList;

        private int _dataBufferSize;

        #region Properties

        /// <summary>
        /// The name of the server on the network when it publishes itself
        /// </summary>
        public string ReadableName
        {
            get { return _readableName; }
        }

        /// <summary>
        /// The port for the server to listen on for incoming connection requests and data streams
        /// </summary>
        public int Port
        {
            get { return _port; }
        }

        /// <summary>
        /// The list of TXTRecords that are published with the server giving more information about the server to devices wishing to connect to it
        /// </summary>
        public TXTRecordList TXTRecordList
        {
            get { return _TXTRecordList; }
        }

        /// <summary>
        /// The size of the buffer of data that the server will receive data to
        /// </summary>
        public int DataBufferSize
        {
            get { return _dataBufferSize; }
        }

        #endregion
        
        /// <summary>
        /// The default constructor for the ServerInfo object
        /// </summary>
        /// <param name="readableName">The name of the server on the network when it publishes itself</param>
        /// <param name="port">The port for the server to listen on for incoming connection requests and data streams</param>
        /// <param name="txtRecordList">The list of TXTRecords that are published with the server giving more information about the server to devices wishing to connect to it</param>
        /// <param name="dataBufferSize">The size of the buffer of data that the server will receive data to</param>
        public ServerInfo(string readableName, int port, TXTRecordList txtRecordList, int dataBufferSize)
        {
            _readableName = readableName;
            _port = port;
            if (txtRecordList == null)
            {
                txtRecordList = new TXTRecordList();
            }
            _TXTRecordList = txtRecordList;
            _dataBufferSize = dataBufferSize;
        }


        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private ServerInfo()
        {

        }

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the server in a readable format</returns>
        public override string ToString()
        {
            return "ServerInfo: name = " + _readableName + "; port = " + _port.ToString() + "; txt records = " + _TXTRecordList.ToString() + "; data buffer size = " + _dataBufferSize.ToString();
        }
    }
}
