namespace Communicate.Serialization
{
    internal class EmptySerializer : IGenericSerializer
    {
        public object FromData(byte[] toDecode, object extra) => null;
        public byte[] ToData(object toEncode, object extra) => null;
    }
}
