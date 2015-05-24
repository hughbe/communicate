using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Common
{
    /// <summary>
    /// An enum describing the type of object a byte array encodes
    /// </summary>
    public enum DataType
    {
        String,
        JSON,
        File,
        Image,
        Other
    }
}
