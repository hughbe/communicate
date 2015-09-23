using System.Drawing;
using System.IO;

namespace Communicate.Serialization
{
    internal class ImageSerializer : IDataSerializer<Image, Image, object>
    {
        public byte[] ToData(Image toEncode, object extra) => (byte[])new ImageConverter().ConvertTo(toEncode, typeof(byte[]));

        public Image FromData(byte[] toDecode, object extra)
        {
            if (toDecode == null)
            {
                return null;
            }
            using (var memoryStream = new MemoryStream(toDecode))
            {
                return Image.FromStream(memoryStream);
            }
        }
    }
}
