using System;
using System.IO;

namespace Communicate.Serialization
{
    internal static class XmlSerializer
    {
        public static byte[] ToData(object value, Type extra)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (extra == null)
            {
                throw new ArgumentNullException(nameof(extra));
            }

            using (var stream = new MemoryStream())
            {
                new System.Xml.Serialization.XmlSerializer(extra).Serialize(stream, value);
                return stream.ToArray();
            }
        }

        public static object FromData(byte[] data, Type extra)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (extra == null)
            {
                throw new ArgumentNullException(nameof(extra));
            }

            using (var stream = new MemoryStream(data))
            {
                return new System.Xml.Serialization.XmlSerializer(extra).Deserialize(stream);
            }
        }
    }
}
