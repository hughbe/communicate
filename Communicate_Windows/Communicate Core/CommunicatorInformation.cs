using System;

namespace Communicate
{
    public class CommunicatorInformation
    {
        public CommunicatorInformation(int port, string name = null)
        {
            Port = port;
            Name = name ?? Environment.MachineName;
        }

        public string Name { get; }
        public int Port { get; }
    }
}