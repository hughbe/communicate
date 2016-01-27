using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Communicate.Serialization
{
    internal static class BinarySerializer
    {
        public static byte[] ToData(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            using (var stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, value);
                return stream.ToArray();
            }
        }

        public static object FromData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (var stream = new MemoryStream(data))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
        }
    }
}
