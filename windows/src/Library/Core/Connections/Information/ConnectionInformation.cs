using System;
using System.Net;

namespace Communicate
{
    public class ConnectionInformation
    {
        internal ConnectionInformation() 
        {
        }
        
        internal ConnectionInformation(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }
            SetEndPoint(endPoint);
        }
        
        internal void SetEndPoint(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException(nameof(endPoint));
            }
            EndPoint = endPoint;
            Resolved = true;
        }

        public IPEndPoint EndPoint { get; private set; }
        public bool Resolved { get; private set; }

        public string Name { get; internal set; }

        public CommunicatorVersion Version { get; }
        public Platform Platform { get; }
    }
}
