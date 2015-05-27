using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Common
{
    /// <summary>
    /// The class that represents the footer of a communication data object
    /// </summary>
    public class DataFooter : DataHeaderFooter
    {
        /// <summary>
        /// Constructs a footer from a given dictionary
        /// </summary>
        /// <param name="footerDictionary">The dictionary from which to create the footer</param>
        public DataFooter(Dictionary<string, string> footerDictionary)
        {
            base.ConstructFromDictionary(footerDictionary);
        }

        /// <summary>
        /// Constructs a footer from a given byte array
        /// </summary>
        /// <param name="footerData">The byte array from which to create the footer</param>
        public DataFooter(byte[] footerData)
        {
            base.ConstructFromData(footerData);
        }

        /// <summary>
        /// Constructs a footer from a given string
        /// </summary>
        /// <param name="footerString">The string from which to create the footer</param>
        public DataFooter(string footerString)
        {
            base.ConstructFromString(footerString);
        }

        /// <summary>
        /// Constructs an empty footer
        /// </summary>
        public DataFooter()
        {
            ConstructFromDictionary(new Dictionary<string, string>());
        }

    }
}
