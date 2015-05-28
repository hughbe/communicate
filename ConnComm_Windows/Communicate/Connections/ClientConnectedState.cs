using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Connections
{
    /// <summary>
    /// An enum that represents the connected state of a connection
    /// </summary>
    public enum ConnectedState
    {
        NotConnected,
        Connecting,
        Connected,
        ErrorConnecting,
        Disconnected
    }
}
