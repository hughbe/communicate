using System;

namespace Communicate.Serialization
{
    internal static class JsonObjectSerializer<T>
    {
        public static byte[] ToData(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return JsonSerializer.ToData(value);
        }

        public static T FromData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            return (T)JsonSerializer.FromData(data, typeof(T));
        }
    }
}
