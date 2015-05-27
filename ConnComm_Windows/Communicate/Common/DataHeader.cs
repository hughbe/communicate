using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Common
{
    /// <summary>
    /// The class that represents the header of a communication data object
    /// </summary>
    public class DataHeader : DataHeaderFooter
    {

        private static string FileNameKey = "FileName";
        private static string JSONObjectTypeKey = "JSONObjectType";
        
        /// <summary>
        /// Constructs a header from a given dictionary
        /// </summary>
        /// <param name="footerDictionary">The dictionary from which to create the header</param>
        public DataHeader(Dictionary<string, string> footerDictionary)
        {
            base.ConstructFromDictionary(footerDictionary);
        }

        /// <summary>
        /// Constructs a header from a given byte array
        /// </summary>
        /// <param name="footerData">The byte array from which to create the header</param>
        public DataHeader(byte[] footerData)
        {
            base.ConstructFromData(footerData);
        }

        /// <summary>
        /// Constructs a header from a given string
        /// </summary>
        /// <param name="footerString">The string from which to create the header</param>
        public DataHeader(string footerString)
        {
            base.ConstructFromString(footerString);
        }

        /// <summary>
        /// Constructs an empty header
        /// </summary>
        public DataHeader()
        {
            ConstructFromDictionary(new Dictionary<string, string>());
        }

        /// <summary>
        /// Gets the file name of a file or image send in the header
        /// </summary>
        public string FileName
        {
            get
            {
                if (FooterDictionary.ContainsKey(FileNameKey))
                {
                    return FooterDictionary[FileNameKey];
                }
                else
                {
                    return "";
                }
            }
            set 
            {
                if (FooterDictionary.ContainsKey(FileNameKey))
                {
                    FooterDictionary[FileNameKey] = value;
                }
                else
                {
                    AddFooterEntry(FileNameKey, value);
                }
            }
        }

        /// <summary>
        /// Gets the type of JSON object received
        /// </summary>
        public JSONObjectType JsonObjectType
        {
            get
            {
                if (FooterDictionary.ContainsKey(FileNameKey))
                {
                    return (JSONObjectType)Enum.Parse(typeof(JSONObjectType), FooterDictionary[JSONObjectTypeKey]);
                }
                else
                {
                    return JSONObjectType.None;
                } 
            }
            set 
            {
                if (FooterDictionary.ContainsKey(JSONObjectTypeKey))
                {
                    FooterDictionary[JSONObjectTypeKey] = Convert.ToString(value);
                }
                else
                {
                    AddFooterEntry(JSONObjectTypeKey, Convert.ToString(value));
                }
            }
        }
    }
}
