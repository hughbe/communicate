using System;

namespace Communicate
{
    public class CommunicatorInformation
    {
        public CommunicatorInformation(int port) : this(port, null)
        {
        }

        public CommunicatorInformation(int port, string name)
        {
            Port = port;
            Name = string.IsNullOrEmpty(name) ? Environment.MachineName : name;
        }

        public string Name { get; }
        public int Port { get; }
    }
}