using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communicate.Serialization
{
    internal class BinarySerializer : IObjectSerializer
    {
        public byte[] ToData(object toEncode, Type extra)
        {
            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, toEncode);
                return stream.ToArray();
            }
        }

        public object FromData(byte[] toDecode, Type extra)
        {
            using (var stream = new MemoryStream(toDecode))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
        }
    }
}
