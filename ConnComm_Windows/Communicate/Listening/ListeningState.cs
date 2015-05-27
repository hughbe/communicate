using System;

namespace Communicate.Listening
{
    /// <summary>
    /// An enum that represents the listening state of a communicator
    /// </summary>
    public enum ListeningState
    {
        NotListening,
        Listening,
        ErrorListening,
        StoppedListening
    }
}
