using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Client
{
    /// <summary>
    /// An enum that represents the connected state of a client
    /// </summary>
    public enum ClientConnectedState
    {
        NotConnected,
        Connecting,
        Connected,
        ErrorConnecting,
        Disconnected
    }
}
