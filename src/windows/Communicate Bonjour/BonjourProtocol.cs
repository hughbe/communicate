namespace Communicate.Bonjour
{
    public class BonjourProtocol : BaseProtocol
    {
        private const string ProtocolDomainLocal = "local";

        public BonjourProtocol(string name, ProtocolTransport transport) : this(name, transport, ProtocolDomainLocal)
        {
        }

        public BonjourProtocol(string name, string domain) : this(name, ProtocolTransport.Tcp, domain)
        {
        }

        /// <summary>
        ///     The default constructor for the ProtocolInformation object. Example use: Protocol("Test",
        ///     ProtocolTransport.Tcp,
        ///     null) would publish a service under the full Bonjour type of "_Test._tcp.local."
        /// </summary>
        /// <param name="name">
        ///     The name of the type section of the service that the server will publish. It should not start with
        ///     an underscore, have no spaces and have no full stops; e.g."Test"
        /// </param>
        /// <param name="transport">
        ///     The type of transport protocol that the server will publish using. It is currently limited to TCP
        ///     only
        /// </param>
        /// <param name="domain">
        ///     The domain of the type section of the service that the server will publish. It should usually be
        ///     left null and contain no spaces or full stops
        /// </param>
        public BonjourProtocol(string name, ProtocolTransport transport = ProtocolTransport.Tcp, string domain = null) : base(name.Replace("_", string.Empty), transport)
        {
            Domain = domain ?? ProtocolDomainLocal;
        }

        public string Domain { get; }

        public override string SerializeType() => "_" + Name + "._" + StringFromTransport(Transport) + ".";
    }
}
