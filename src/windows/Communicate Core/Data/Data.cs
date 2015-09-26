using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Communicate
{
    public class Data
    {
        internal Data(DataInfo info)
        {
            Info = info;
        }

        internal Data(DataType type)
        {
            Construct(type, null, null, null);
        }

        public Data(DataType type, byte[] content)
        {
            Construct(type, null, content, null);
        }

        public Data(DataType type, DataHeaderFooter header, byte[] content, DataHeaderFooter footer)
        {
            Construct(type, header, content, footer);
        }

        public Data(string theString, Encoding encoding = null, DataType type = null)
        {
            type = type ?? DataType.Text;
            Construct(type, null, type.Serialize(theString, encoding), null);
        }

        public Data(Image image, string name)
        {
            var header = new DataHeaderFooter {FileName = name};
            Construct(DataType.Image, header, DataType.Image.Serialize(image), null);
        }

        public Data(string filePath, string name)
        {
            var type = DataType.File;
            var header = new DataHeaderFooter { FileName = name };

            if (IsValidImage(File.ReadAllBytes(filePath)))
            {
                Construct(DataType.Image, header, DataType.Image.Serialize(Image.FromFile(filePath)), null);
            }
            else
            {
                Construct(DataType.File, header, DataType.File.Serialize(filePath), null);
            }
        }

        public Data(object objectToEncode, DataType dataType)
        {
            if (!dataType.IsSupported)
            {
                return;
            }

            Construct(dataType, null, dataType.Serialize(dataType, objectToEncode.GetType()), null);
        }

        public Data(byte[] data)
        {
            Construct(DataType.Other, null, data, null);
        }

        public DataInfo Info { get; internal set; }

        public DataHeaderFooter Header { get; set; }
        public byte[] Content { get; set; }
        public DataHeaderFooter Footer { get; set; }

        private void Construct(DataType type, DataHeaderFooter header, byte[] content, DataHeaderFooter footer)
        {
            Header = header ?? new DataHeaderFooter();
            Content = content ?? new byte[0];
            Footer = footer ?? new DataHeaderFooter();
            Info = new DataInfo(type, header, content, footer);
        }

        private static bool IsValidImage(byte[] bytes)
        {
            try
            {
                using (var memoryStream = new MemoryStream(bytes))
                {
                    Image.FromStream(memoryStream);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string GetString(Encoding encoding = null) => DataType.Text.Deserialize<string>(Content, encoding);

        public Image GetImage() => DataType.Image.Deserialize<Image>(Content);

        public List<T> GetArray<T>() => GetObject<List<T>>();

        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>() => GetObject<Dictionary<TKey, TValue>>();

        public T GetObject<T>() => GetDataType().Deserialize<T>(Content, typeof(T));
        
        public DataType GetDataType() => Info.Type;
    }
}