namespace Communicate
{
    public enum CommunicatorErrorCode
    {
        None = 0,

        ListeningSocketCouldNotOpen,
        ListeningSocketClosed,
        ListeningUnknownError,

        PublishingNotSupported,
        PublishingTimedOut,
        PublishingAlreadyRegistered,
        PublishingNamingCollision,
        PublishingFirewallBlocked,
        PublishingUnknownError,

        SearchingNotSupported,
        SearchingTimedOut,
        SearchingFirewallBlocked,
        SearchingUnknownError,

        ResolvingTimedOut,
        ResolvingUnknownError,

        ConnectionRejected,
        ConnectionClosed,
        ConnectionSocketCreationError,
        ConnectionUnknownError
    }
}
