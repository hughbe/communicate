namespace Communicate
{
    public enum ProtocolTransport
    {
        Tcp
    }

    public class Protocol
    {
        public string Name { get; }
        public ProtocolTransport Transport { get; }

        public Protocol(string name) : this(name, ProtocolTransport.Tcp)
        {
        }

        protected Protocol(string name, ProtocolTransport transport)
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