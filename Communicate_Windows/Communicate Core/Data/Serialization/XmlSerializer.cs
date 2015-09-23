using System;
using System.IO;

namespace Communicate.Serialization
{
    internal class XmlSerializer : IObjectSerializer
    {
        public byte[] ToData(object toEncode, Type extra)
        {
            using (var stream = new MemoryStream())
            {
                new System.Xml.Serialization.XmlSerializer(extra).Serialize(stream, toEncode);
                return stream.ToArray();
            }
        }

        public object FromData(byte[] toDecode, Type extra)
        {
            using (var stream = new MemoryStream(toDecode))
            {
                return new System.Xml.Serialization.XmlSerializer(extra).Deserialize(stream);
            }
        }
    }


}
