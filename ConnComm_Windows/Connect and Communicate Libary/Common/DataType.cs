using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Common
{
    /// <summary>
    /// An enum describing the type of object a byte array encodes
    /// </summary>
    public enum CommunicationDataType
    {
        String,
        JSON,
        File,
        Image,
        Other
    }

    /// <summary>
    /// An enum describing the type of object a JSON entry encodes
    /// </summary>
    public enum JSONObjectType
    {
        None,
        Dictionary,
        Array,
        Other
    }
}
