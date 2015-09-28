using System;
using System.Linq;

namespace Communicate
{
    internal class DataInfo
    {
        internal DataInfo(byte[] bytes) : this(new DataType(BitConverter.ToInt32(bytes, 0)))
        {
            HeaderLength = BitConverter.ToInt32(bytes, 4);
            ContentLength = BitConverter.ToInt32(bytes, 8);
            FooterLength = BitConverter.ToInt32(bytes, 12);
        }

        internal DataInfo(DataType dataType)
        {
            DataType = dataType;
        }

        public DataType DataType { get; internal set; }

        public int HeaderLength { get; internal set; }
        public int ContentLength { get; internal set; }
        public int FooterLength { get; internal set; }

        public static int DataInfoSize { get; } = 16;

        private static int GetLength(byte[] data) => data?.Length ?? 0;

        public byte[] GetData() =>
            Combine(DataType.GetBytes(),
                    BitConverter.GetBytes(HeaderLength),
                    BitConverter.GetBytes(ContentLength),
                    BitConverter.GetBytes(FooterLength));

        private static byte[] Combine(params byte[][] byteArrays)
        {
            var combined = new byte[byteArrays.Sum(a => a.Length)];
            var offset = 0;
            foreach (var array in byteArrays)
            {
                Buffer.BlockCopy(array, 0, combined, offset, array.Length);
                offset += array.Length;
            }
            return combined;
        }
    }
}