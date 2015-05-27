using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Communicate.Common
{
    /// <summary>
    /// A class that contains all information about a footer for a data communication
    /// </summary>
    public abstract class DataHeaderFooter
    {
        #region Private Variables

        private Dictionary<string, string> _footerDictionary;

        private static string FileNameKey = "FileName";

        #endregion

        #region Properties

        /// <summary>
        /// The inner storage of the footer data
        /// </summary>
        public Dictionary<string, string> FooterDictionary
        {
            get { return _footerDictionary; }
        }

        #endregion
        
        /// <summary>
        /// Constructs a footer from a given dictionary
        /// </summary>
        /// <param name="dictionary">The dictionary from which to create the footer</param>
        internal void ConstructFromDictionary(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, string>();
            }
            _footerDictionary = dictionary;
        }

        /// <summary>
        /// Constructs a footer from a given byte array
        /// </summary>
        /// <param name="data">The byte array from which to create the footer</param>
        internal void ConstructFromData(byte[] data)
        {
            string footerString = "";
            if (data.Length > 0)
            {
                footerString = DataSerializers.StringSerializer.StringFromByteArray(data);
            }
            ConstructFromString(footerString);
        }

        /// <summary>
        /// Constructs a footer from a given string
        /// </summary>
        /// <param name="footerString">The string from which to create the footer</param>
        internal void ConstructFromString(string footerString)
        {
            if (footerString == null)
            {
                _footerDictionary = new Dictionary<string, string>();
            }
            else
            {
                try
                {
                    _footerDictionary = new JavaScriptSerializer().Deserialize<Dictionary<string, string>>(footerString);
                }
                catch
                {
                    _footerDictionary = new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// Constructs a default empty footer
        /// </summary>
        public DataHeaderFooter()
        {
            _footerDictionary = new Dictionary<string, string>();
        }

        /// <summary>
        /// Serializes the footer into a string
        /// </summary>
        /// <returns>A JSON string containing the footer data</returns>
        public string GetJSON()
        {
            if (_footerDictionary.Count > 0)
            {
                return new JavaScriptSerializer().Serialize(_footerDictionary);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Serializes the footer into a byte array
        /// </summary>
        /// <returns>A byte array containing the footer data</returns>
        public byte[] GetBytes()
        {
            return Encoding.ASCII.GetBytes(GetJSON());
        }

        #region Accessors and Mutators
                /// <summary>
        /// Gets the value for a specified key
        /// </summary>
        /// <param name="key">The key to get the value for</param>
        /// <returns>The value of a specified key</returns>
        public string ValueForKey(string key)
        {
            return _footerDictionary[key];
        }

        /// <summary>
        /// Sets the value of a specified key
        /// </summary>
        /// <param name="value">The new value for the key</param>
        /// <param name="key">The key to set the value for</param>
        public void SetValueForKey(string value, string key)
        {
            _footerDictionary[key] = value;
        }

        /// <summary>
        /// Gets the integer value for a specified key
        /// </summary>
        /// <param name="key">The key to get the value for</param>
        /// <returns>The integer value of a specified key</returns>
        public int IntegerForKey(string key)
        {
            return Convert.ToInt32(_footerDictionary[key]);
        }

        /// <summary>
        /// Sets the integer value of a specified key
        /// </summary>
        /// <param name="value">The new integer value for the key</param>
        /// <param name="key">The key to set the value for</param>
        public void SetIntegerForKey(int value, string key)
        {
            _footerDictionary[key] = Convert.ToString(value);
        }

        /// <summary>
        /// Gets the bool value for a specified key
        /// </summary>
        /// <param name="key">The key to get the value for</param>
        /// <returns>The bool value of a specified key</returns>
        public bool BoolForKey(string key)
        {
            return Convert.ToBoolean(_footerDictionary[key]);
        }

        /// <summary>
        /// Sets the bool value of a specified key
        /// </summary>
        /// <param name="value">The new bool value for the key</param>
        /// <param name="key">The key to set the value for</param>
        public void SetBoolForKey(bool value, string key)
        {
            _footerDictionary[key] = Convert.ToString(value);
        }

        /// <summary>
        /// Adds a key value pair to the footer record list
        /// </summary>
        /// <param name="key">The key of the new footer record</param>
        /// <param name="value">The value of the new footer record</param>
        public void AddFooterEntry(string key, string value)
        {
            _footerDictionary.Add(key, value);
        }

        /// <summary>
        /// Adds a key value pair to the footer record list
        /// </summary>
        /// <param name="key">The key of the new footer record</param>
        /// <param name="value">The integer value of the new footer record</param>
        public void AddFooterEntry(string key, int value)
        {
            _footerDictionary.Add(key, Convert.ToString(value));
        }

        /// <summary>
        /// Adds a key value pair to the footer record list
        /// </summary>
        /// <param name="key">The key of the new footer record</param>
        /// <param name="value">The bool value of the new footer record</param>
        public void AddFooterEntry(string key, bool value)
        {
            _footerDictionary.Add(key, Convert.ToString(value));
        }
        #endregion

        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the footer in a readable format</returns>
        public override string ToString()
        {
            return "Header/Footer: entries = " + _footerDictionary.ToString();
        }
    }
}
