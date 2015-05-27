using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Publishing
{
    /// <summary>
    /// An enum that represents the publishing state of a communicator
    /// </summary>
    public enum PublishingState
    {
        NotPublished,
        Publishing,
        Published,
        ErrorPublishing,
        UnPublished
    }
}
