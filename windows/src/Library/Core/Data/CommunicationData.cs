using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Text;

namespace Communicate
{
    public class CommunicationData
    {
        public CommunicationData()
        {
        }

        public CommunicationData(DataType dataType)
        {
            if (dataType == null)
            {
                throw new ArgumentNullException(nameof(dataType));
            }
            Info.DataType = dataType;
        }

        internal CommunicationData(DataInfo dataInfo)
        {
            if (dataInfo == null)
            {
                throw new ArgumentNullException(nameof(dataInfo));
            }
            Info = dataInfo;
        }

        internal DataInfo Info { get; private set; }

        public DataHeaderFooter Header { get; internal set; } = new DataHeaderFooter();
        public DataHeaderFooter Footer { get; internal set; } = new DataHeaderFooter();

        internal byte[] InternalContent { get; set; }
        
        public CommunicationData WithHeader(DataHeaderFooter header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }
            Header = header;
            return this;
        }
        
        public CommunicationData WithHeader(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            Header.SetValueForKey(value, key);
            return this;
        }

        public CommunicationData WithName(string name) => WithHeader(DataHeaderFooter.NameKey, name);
        public CommunicationData WithPath(string path) => WithHeader(DataHeaderFooter.PathKey, path);

        public CommunicationData WithFooter(DataHeaderFooter footer)
        {
            if (footer == null)
            {
                throw new ArgumentNullException(nameof(footer));
            }
            Footer = footer;
            return this;
        }

        public CommunicationData WithContent(byte[] data, DataType type)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            InternalContent = data;
            Info = new DataInfo(type);
            return this;
        }

        public CommunicationData WithData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            return WithContent(data, DataType.Other);
        }

        private CommunicationData Serialize(object value, object extra, DataType type)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return WithContent(type.Serialize(value, extra), type);
        }

        public CommunicationData WithImage(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return Serialize(image, null, DataType.Image);
        }

        public CommunicationData WithFilePath(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }
            else if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The file to serialize was not found", filePath);
            }

            return Serialize(filePath, null, DataType.File);
        }

        public CommunicationData WithString(string value) => WithString(value, null);

        public CommunicationData WithString(string value, Encoding encoding)
        { 
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            return Serialize(value, encoding ?? Encoding.ASCII, DataType.Text);
        }

        public CommunicationData WithEncodedString(string value, EncodedDataType encodedDataType)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            DataType dataType = null;
            switch (encodedDataType)
            {
                case EncodedDataType.Json:
                    dataType = DataType.JsonString;
                    break;
                case EncodedDataType.Xml:
                    dataType = DataType.XmlString;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapString;
                    break;
                default:
                    throw new ArgumentException("This data type is not supported by Communicate on Windows, or is not string encodable.", nameof(encodedDataType));
            }
            return Serialize(value, value.GetType(), dataType);
        }

        public CommunicationData WithObject(object value, EncodedDataType encodedDataType)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            DataType dataType = null;
            switch (encodedDataType)
            {
                case EncodedDataType.Json:
                    dataType = DataType.JsonObject;
                    break;
                case EncodedDataType.Xml:
                    dataType = DataType.XmlObject;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapObject;
                    break;
                case EncodedDataType.Binary:
                    dataType = DataType.BinaryObject;
                    break;
                default:
                    throw new ArgumentException("This data type is not supported by Communicate on Windows, or is not object encodable.", nameof(encodedDataType));
            }
            return Serialize(value, value.GetType(), dataType);
        }

        public CommunicationData WithDictionary(IDictionary dictionary, EncodedDataType encodedDataType)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            DataType dataType = null;
            switch (encodedDataType)
            {
                case EncodedDataType.Json:
                    dataType = DataType.JsonDictionary;
                    break;
                case EncodedDataType.Xml:
                    dataType = DataType.XmlDictionary;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapDictionary;
                    break;
                case EncodedDataType.Binary:
                    dataType = DataType.BinaryDictionary;
                    break;
                default:
                    throw new ArgumentException("This data type is not supported by Communicate on Windows, or is not object encodable.", nameof(encodedDataType));
            }
            return Serialize(dictionary, dictionary.GetType(), dataType);
        }

        public CommunicationData WithList(IList list, EncodedDataType encodedDataType)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            DataType dataType = null;
            switch (encodedDataType)
            {
                case EncodedDataType.Json:
                    dataType = DataType.JsonArray;
                    break;
                case EncodedDataType.Xml:
                    dataType = DataType.XmlArray;
                    break;
                case EncodedDataType.Soap:
                    dataType = DataType.SoapArray;
                    break;
                case EncodedDataType.Binary:
                    dataType = DataType.BinaryArray;
                    break;
                default:
                    throw new ArgumentException("This data type is not supported by Communicate on Windows, or is not object encodable.", nameof(encodedDataType));
            }
            return Serialize(list, list.GetType(), dataType);
        }

        internal void PrepareForSending()
        {
            Info.HeaderLength = GetLength(Header.GetData());
            Info.ContentLength = GetLength(InternalContent);
            Info.FooterLength = GetLength(Footer.GetData());
        }

        private int GetLength(byte[] data) => data?.Length ?? 0;

        public T GetObject<T>(DataType dataType)
        {
            if (dataType == null)
            {
                dataType = DataType;
            }
            return dataType.Deserialize<T>(GetData(), null);
        }

        public byte[] GetData() => InternalContent;

        public string GetString() => GetString(null);
        public string GetString(Encoding encoding) => DataType.Text.Deserialize<string>(GetData(), encoding ?? Encoding.ASCII);

        public Image GetImage() => GetImage(false);
        public Image GetImage(bool fromFilePath)
        {
            if (fromFilePath)
            {
                return Image.FromFile(Header.Path);
            }
            else
            {
                return DataType.Image.Deserialize<Image>(GetData());
            }
        }

        public T GetObject<T>() => DataType.Deserialize<T>(GetData(), typeof(T));

        public Collection<T> GetArray<T>() => GetObject<Collection<T>>();

        public Dictionary<TKey, TValue> GetDictionary<TKey, TValue>() => GetObject<Dictionary<TKey, TValue>>();
                
        public DataType DataType => Info.DataType;
    }
}