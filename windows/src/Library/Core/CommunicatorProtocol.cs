namespace Communicate
{
    public enum ProtocolTransport
    {
        Tcp
    }

    public class CommunicatorProtocol
    {
        public string Name { get; }
        public ProtocolTransport Transport { get; }

        public CommunicatorProtocol(string name) : this(name, ProtocolTransport.Tcp)
        {
        }

        protected CommunicatorProtocol(string name, ProtocolTransport transport)
        {
            Name = name;
            Transport = transport;
        }

        public string TransportString
        {
            get
            {
                var toReturn = "";
                if (Transport == ProtocolTransport.Tcp)
                {
                    toReturn = "tcp";
                }
                return toReturn;
            }
        }
    }
}