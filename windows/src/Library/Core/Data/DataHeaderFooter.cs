using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web.Script.Serialization;

namespace Communicate
{
    public class DataHeaderFooter
    {
        internal const string NameKey = "Name";
        internal const string PathKey = "Path";

        public DataHeaderFooter()
        {
            Entries = new Dictionary<string, string>();
        }

        public DataHeaderFooter(Dictionary<string, string> entries)
        {
            Entries = entries ?? new Dictionary<string, string>();
        }

        public DataHeaderFooter(byte[] data) : this(Encoding.ASCII.GetString(data))
        {
        }

        private DataHeaderFooter(string entriesString)
        {
           Entries = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(entriesString) ?? new Dictionary<string, string>();
        }

        public Dictionary<string, string> Entries { get; }

        public string Name
        {
            get { return ValueForKey(NameKey); }
            set { SetValueForKey(value, NameKey); }
        }

        public string Path
        {
            get { return ValueForKey(PathKey); }
            set { SetValueForKey(value, PathKey); }
        }

        private string GetJsonString()
        {
            if (Entries != null && Entries.Count > 0)
            {
                return new JavaScriptSerializer().Serialize(Entries);
            }
            return "";
        }

        public byte[] GetData() => Encoding.ASCII.GetBytes(GetJsonString());

        public string ValueForKey(string key) => Entries.ContainsKey(key) ? Entries[key] : null;

        public void SetValueForKey(string value, string key)
        {
            if (Entries.ContainsKey(key))
            {
                Entries[key] = value;
            }
            else
            {
                Entries.Add(key, value);
            }
        }

        public T ValueForKey<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var type = typeof(T);

            var typeConverter = TypeDescriptor.GetConverter(type);
            if (!typeConverter.CanConvertFrom(typeof(string)))
            {
                throw new NotSupportedException("This type cannot be stored in a data header or footer");
            }

            var value = ValueForKey(key);
            if (value == null)
            {
                return default(T);
            }

            return (T)typeConverter.ConvertFromInvariantString(value);
        }

        public void SetValueForKey<T>(T value, string key)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var type = typeof(T);

            var typeConverter = TypeDescriptor.GetConverter(typeof(string));
            if (!typeConverter.CanConvertTo(type))
            {
                throw new NotSupportedException("This type cannot be stored in a data header or footer");
            }
            SetValueForKey(typeConverter.ConvertToInvariantString(value), key);
        }
    }
}