namespace Communicate.Serialization
{
    internal static class EmptySerializer
    {
        public static byte[] ToData() => null;
        public static object FromData() => null;
    }
}
