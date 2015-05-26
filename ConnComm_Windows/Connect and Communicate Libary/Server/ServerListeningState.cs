﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Server
{
    /// <summary>
    /// An enum that represents the publishing state of a server
    /// </summary>
    public enum ServerPublishingState
    {
        NotPublished,
        Publishing,
        Published,
        ErrorPublishing,
        UnPublished
    }
}
