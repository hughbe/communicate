using System.IO;

namespace Communicate.Serialization
{
    internal class FileSerializer : IDataSerializer<string, byte[], object>
    {
        public byte[] ToData(string toEncode, object extra) => File.ReadAllBytes(toEncode);
        public byte[] FromData(byte[] toDecode, object extra) => toDecode;
    }
}
