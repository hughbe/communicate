using System;
using System.Linq;

namespace Communicate
{
    public class DataInfo
    {
        public DataInfo(DataType dataType, DataHeaderFooter header, byte[] content, DataHeaderFooter footer) : 
            this(dataType, 
                Platform.Windows,
                VersionInfo.CurrentVersion,
                GetLength(header?.GetData()), 
                GetLength(content),
                GetLength(footer?.GetData()))
        {
        }

        internal DataInfo(byte[] bytes) :
            this(new DataType(BitConverter.ToInt32(bytes, 0)),
                new Platform(BitConverter.ToInt32(bytes, 4)),
                new VersionInfo(BitConverter.ToInt32(bytes, 8)),
                BitConverter.ToInt32(bytes, 12),
                BitConverter.ToInt32(bytes, 16),
                BitConverter.ToInt32(bytes, 20))
        {
        }

        private DataInfo(DataType dataType, Platform platform, VersionInfo version, int headerLength, int contentLength, int footerLength)
        {
            Type = dataType;
            Platform = platform;
            Version = version;
            HeaderLength = headerLength;
            ContentLength = contentLength;
            FooterLength = footerLength;
        }

        public DataType Type { get; }

        public Platform Platform { get; }
        public VersionInfo Version { get; }

        public int HeaderLength { get; }
        public int ContentLength { get; }
        public int FooterLength { get; }

        public static int DataInfoSize { get; } = 24;

        private static int GetLength(byte[] data) => data?.Length ?? 0;

        public byte[] GetData() =>
            Combine(Type.GetBytes(), 
                    Platform.GetBytes(),
                    Version.GetBytes(),
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