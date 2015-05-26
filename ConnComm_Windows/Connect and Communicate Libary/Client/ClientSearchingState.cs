using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Client
{
    /// <summary>
    /// An enum that represents the searching state of a client
    /// </summary>
    public enum ClientSearchingState
    {
        NotSearching,
        Searching,
        ErrorSearching,
        StoppedSearching
    }
}
