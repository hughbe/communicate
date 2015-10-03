using System;
using Communicate.Serialization;

namespace Communicate
{
    public class DataType : RegisteredObject<DataType>
    {
        public DataType(int identifier) : this(identifier, null)
        {
        }

        public DataType(int identifier, string name) : base(identifier, name)
        {
            if (name == null)
            {
                SerializerType = RegisteredVersion?.SerializerType;
            }
        }

        public Type SerializerType { get; private set; }

        public DataType Register(Type serializerType) 
        {
            if (serializerType == null)
            {
                throw new ArgumentNullException(nameof(serializerType));
            }
            SerializerType = serializerType;
            base.Register();
            return this;
        }

        public override DataType Register() => Register(typeof(EmptySerializer));

        public T Deserialize<T>(byte[] value) => Deserialize<T>(value, null);
        public T Deserialize<T>(byte[] value, object extra) => PerformMethod<T>("FromData", value, extra);

        public byte[] Serialize(object value) => Serialize(value, null);
        public byte[] Serialize(object value, object extra) => PerformMethod<byte[]>("ToData", value, extra);

        private T PerformMethod<T>(string methodName, params object[] arguments)
        {
            var method = SerializerType.GetMethod(methodName);

            var numberOfParameters = method.GetParameters().Length;
            if (numberOfParameters == 0)
            {
                arguments = null;
            }
            else if (numberOfParameters == 1)
            {
                var newParameters = new object[1];
                Array.Copy(arguments, newParameters, 1);
                arguments = newParameters;
            }
            return (T)method.Invoke(null, arguments);
        }
        
        public bool IsStringEncoded => this == Text || IsJson || IsXml || IsSoap;

        public bool IsEncoded => IsJson || IsXml || IsSoap || IsBinary || IsKeyed;

        public bool IsJson => Is(JsonString, JsonObject, JsonDictionary, JsonArray);
        public bool IsXml => Is(XmlString, XmlObject, XmlDictionary, XmlArray);
        public bool IsSoap => Is(SoapString, SoapObject, SoapDictionary, SoapArray);

        public bool IsBinary => Is(BinaryObject, BinaryDictionary, BinaryArray);
        public bool IsKeyed => Is(KeyedObject, KeyedDictionary, KeyedArray);

        public bool IsObject => Is(JsonObject, XmlObject, SoapObject, BinaryObject, KeyedObject);

        public bool IsArray => Is(JsonArray, XmlArray, SoapArray, BinaryArray, KeyedArray);
        public bool IsDictionary => Is(JsonDictionary, XmlDictionary, SoapDictionary, BinaryDictionary, KeyedDictionary);

        public bool IsSupported => !(IsKeyed);

        public static DataType Text => new DataType(1, "Text").Register(typeof(StringSerializer));
        public static DataType Image => new DataType(2, "Image").Register(typeof(ImageSerializer));
        public static DataType File => new DataType(3, "File").Register(typeof(FileSerializer));

        public static DataType JsonString => new DataType(20, "Json String").Register(typeof(JsonSerializer));
        public static DataType JsonObject => new DataType(21, "Json Object").Register(typeof(JsonSerializer));
        public static DataType JsonDictionary => new DataType(22, "Json Dictionary").Register(typeof(JsonSerializer));
        public static DataType JsonArray => new DataType(23, "Json Array").Register(typeof(JsonSerializer));

        public static DataType XmlString => new DataType(30, "Xml String").Register(typeof(XmlSerializer));
        public static DataType XmlObject => new DataType(31, "Xml Object").Register(typeof(XmlSerializer));
        public static DataType XmlDictionary => new DataType(32, "Xml Dictionary").Register(typeof(XmlSerializer));
        public static DataType XmlArray => new DataType(33, "Xml Array").Register(typeof(XmlSerializer ));

        public static DataType SoapString => new DataType(40, "Soap String").Register(typeof(SoapSerializer));
        public static DataType SoapObject => new DataType(41, "Soap Object").Register(typeof(SoapSerializer));
        public static DataType SoapDictionary => new DataType(42, "Soap Dictionary").Register(typeof(SoapSerializer));
        public static DataType SoapArray => new DataType(44, "Soap Array").Register(typeof(SoapSerializer));

        public static DataType BinaryObject => new DataType(51, "Binary Object").Register(typeof(BinarySerializer));
        public static DataType BinaryDictionary => new DataType(52, "Binary Dictionary").Register(typeof(BinarySerializer));
        public static DataType BinaryArray => new DataType(53, "Binary Array").Register(typeof(BinarySerializer));

        public static DataType KeyedObject => new DataType(61, "Keyed Object").Register();
        public static DataType KeyedDictionary => new DataType(62, "Keyed Dictionary").Register();
        public static DataType KeyedArray => new DataType(63, "Keyed Array").Register();

        public static DataType Other => new DataType(99, "Other").Register();
        public static DataType ConnectionInformation => new DataType(100, "Connection Information").Register(typeof(InformationSerializer)).Register();
        public static DataType Termination => new DataType(0, "Termination").Register();
    }
}