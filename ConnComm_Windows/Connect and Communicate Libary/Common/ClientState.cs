using System;

namespace Communicate.Common
{
    /// <summary>
    /// The enum that dictates the current state of a client
    /// </summary>
    public enum ClientState
    {
        NotConnected,
        Connecting,
        Connected,
        Disconnected
    }
}
