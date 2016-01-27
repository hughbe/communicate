using System;
using System.IO;

namespace Communicate.Serialization
{
    internal static class FileSerializer
    {
        public static byte[] ToData(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return File.ReadAllBytes(value);
        }

        public static byte[] FromData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return data;
        }
    }
}
