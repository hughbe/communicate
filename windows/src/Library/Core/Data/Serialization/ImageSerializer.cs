using System;
using System.Drawing;
using System.IO;

namespace Communicate.Serialization
{
    internal static class ImageSerializer
    {
        public static byte[] ToData(Image value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return (byte[])new ImageConverter().ConvertTo(value, typeof(byte[]));
        }

        public static Image FromData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            using (var memoryStream = new MemoryStream(data))
            {
                return Image.FromStream(memoryStream);
            }
        }
    }
}
