using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;

namespace Communicate.Serialization
{
    internal class SoapSerializer : IObjectSerializer
    {
        public byte[] ToData(object toEncode, Type extra)
        {
            using (var stream = new MemoryStream())
            {
                new SoapFormatter().Serialize(stream, toEncode);
                return stream.ToArray();
            }
        }

        public object FromData(byte[] toDecode, Type extra)
        {
            using (var stream = new MemoryStream(toDecode))
            {
                return new SoapFormatter().Deserialize(stream);
            }
        }
    }
}
