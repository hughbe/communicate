namespace Communicate
{
    public enum ProtocolTransport
    {
        Tcp
    }

    public abstract class BaseProtocol
    {
        public string Name { get; protected set; }
        public ProtocolTransport Transport { get; protected set; }

        protected BaseProtocol(string name, ProtocolTransport transport)
        {
            Name = name;
            Transport = transport;
        }

        public abstract string SerializeType();

        protected static string StringFromTransport(ProtocolTransport type)
        {
            var toReturn = "";
            if (type == ProtocolTransport.Tcp)
            {
                toReturn = "tcp";
            }
            return toReturn;
        }
    }
}