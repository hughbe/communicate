using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Script.Serialization;

namespace Communicate
{
    public class DataHeaderFooter
    {
        private const string NameKey = "Name";

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
            try
            {
                Entries = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(entriesString);
            }
            catch
            {
                Entries = new Dictionary<string, string>();
            }
        }

        public Dictionary<string, string> Entries { get; }

        public string FileName
        {
            get { return ValueForKey(NameKey); }
            set { SetValueForKey(value, NameKey); }
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

        public string ValueForKey(string key) => Entries.ContainsKey(key) ? Entries[key] : "";

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

        public int IntegerForKey(string key) => Convert.ToInt32(ValueForKey(key));

        public void SetIntegerForKey(int value, string key) => SetValueForKey(Convert.ToString(value), key);

        public float FloatForKey(string key) => (float) Convert.ToDouble(ValueForKey(key));

        public void SetFloatForKey(float value, string key) => SetValueForKey(Convert.ToString(value, CultureInfo.CurrentCulture), key);

        public bool BoolForKey(string key) => Convert.ToBoolean(ValueForKey(key));

        public void SetBoolForKey(bool value, string key) => SetValueForKey(Convert.ToString(value), key);
    }
}