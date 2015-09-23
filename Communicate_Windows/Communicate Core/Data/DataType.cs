using System;
using Communicate.Serialization;

namespace Communicate
{
    public class DataType : RegisteredObject<DataType>
    {
        public DataType(int identifer, string name = null) : base(identifer, name)
        {
        }

        public Type SerializerType { get; private set; }

        public DataType Register<TSerializer>() where TSerializer: IDataSerializer
        {
            base.Register();
            SerializerType = typeof(TSerializer);
            return this;
        }

        private new void Register()
        {
        }

        protected override void SetupFromRegistry(DataType registeredObject)
        {
            SerializerType = registeredObject.SerializerType;
            base.SetupFromRegistry(registeredObject);
        }

        public T Deserialize<T>(byte[] toDeserialize, object extra = null) => PerformMethod<T>("FromData", toDeserialize, extra);
        public byte[] Serialize(object toSerialize, object extra = null) => PerformMethod<byte[]>("ToData", toSerialize, extra);

        private T PerformMethod<T>(string methodName, params object[] arguments)
        {
            var method = SerializerType.GetMethod(methodName);
            var serializer = Activator.CreateInstance(SerializerType);

            return (T)method.Invoke(serializer, arguments);
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

        public static DataType Text => new DataType(1, "Text").Register<StringSerializer>();
        public static DataType Image => new DataType(2, "Image").Register<ImageSerializer>();
        public static DataType File => new DataType(3, "File").Register<FileSerializer>();

        public static DataType JsonString => new DataType(20, "Json String").Register<JsonSerializer>();
        public static DataType JsonObject => new DataType(21, "Json Object").Register<JsonSerializer>();
        public static DataType JsonDictionary => new DataType(22, "Json Dictionary").Register<JsonSerializer>();
        public static DataType JsonArray => new DataType(23, "Json Array").Register<JsonSerializer>();

        public static DataType XmlString => new DataType(30, "Xml String").Register<XmlSerializer>();
        public static DataType XmlObject => new DataType(31, "Xml Object").Register<XmlSerializer>();
        public static DataType XmlDictionary => new DataType(32, "Xml Dictionary").Register<XmlSerializer>();
        public static DataType XmlArray => new DataType(33, "Xml Array").Register<XmlSerializer>();

        public static DataType SoapString => new DataType(40, "Soap String").Register<SoapSerializer>();
        public static DataType SoapObject => new DataType(41, "Soap Object").Register<SoapSerializer>();
        public static DataType SoapDictionary => new DataType(42, "Soap Dictionary").Register<SoapSerializer>();
        public static DataType SoapArray => new DataType(44, "Soap Array").Register<SoapSerializer>();

        public static DataType BinaryObject => new DataType(51, "Binary Object").Register<BinarySerializer>();
        public static DataType BinaryDictionary => new DataType(52, "Binary Dictionary").Register<BinarySerializer>();
        public static DataType BinaryArray => new DataType(53, "Binary Array").Register<BinarySerializer>();

        public static DataType KeyedObject => new DataType(61, "Keyed Object").Register<EmptySerializer>();
        public static DataType KeyedDictionary => new DataType(62, "Keyed Dictionary").Register<EmptySerializer>();
        public static DataType KeyedArray => new DataType(63, "Keyed Array").Register<EmptySerializer>();

        public static DataType Other => new DataType(99, "Other").Register<EmptySerializer>();
        public static DataType Termination => new DataType(0, "Termination").Register<EmptySerializer>();
    }
}