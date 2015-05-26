using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Server
{
    /// <summary>
    /// An enum that represents the listening state of a server
    /// </summary>
    public enum ServerListeningState
    {
        NotListening,
        Listening,
        ErrorListening,
        StoppedListening
    }
}
