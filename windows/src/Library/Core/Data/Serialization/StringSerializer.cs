using System;
using System.Text;

namespace Communicate.Serialization
{
    internal static class StringSerializer
    {
        public static byte[] ToData(string value, Encoding extra)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (extra == null)
            {
                throw new ArgumentNullException(nameof(extra));
            }

            return extra.GetBytes(value);

        }
        public static string FromData(byte[] data, Encoding extra)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (extra == null)
            {
                throw new ArgumentNullException(nameof(extra));
            }

            return extra.GetString(data);
        }
    }
}
