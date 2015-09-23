using System.Text;

namespace Communicate.Serialization
{
    internal class StringSerializer : IDataSerializer<string, string, Encoding>
    {
        public byte[] ToData(string toEncode, Encoding extra) => (extra ?? Encoding.ASCII).GetBytes(toEncode ?? "");
        public string FromData(byte[] toDecode, Encoding extra) => (extra ?? Encoding.ASCII).GetString(toDecode);
    }
}
