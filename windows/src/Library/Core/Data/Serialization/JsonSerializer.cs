using System;
using System.Text;
using System.Web.Script.Serialization;

namespace Communicate.Serialization
{
    internal static class JsonSerializer
    {
        public static byte[] ToData(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return StringSerializer.ToData(new JavaScriptSerializer().Serialize(value), Encoding.ASCII);
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
            var stringData = StringSerializer.FromData(data, Encoding.ASCII);
            return new JavaScriptSerializer().Deserialize(stringData, extra);
        }
    }
}
