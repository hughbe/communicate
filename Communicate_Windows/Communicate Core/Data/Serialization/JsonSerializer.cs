using System;
using System.Text;
using System.Web.Script.Serialization;

namespace Communicate.Serialization
{
    internal class JsonSerializer : IObjectSerializer
    {
        public byte[] ToData(object toEncode, Type extra) => new StringSerializer().ToData(new JavaScriptSerializer().Serialize(toEncode), Encoding.ASCII);

        public object FromData(byte[] toDecode, Type extra)
        {
            var stringData = new StringSerializer().FromData(toDecode, Encoding.ASCII);
            return new JavaScriptSerializer().Deserialize(stringData, extra);
        }
    }
}
