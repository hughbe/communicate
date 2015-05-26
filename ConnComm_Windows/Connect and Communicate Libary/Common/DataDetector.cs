using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Communicate.Common
{
    /// <summary>
    /// A class to detect the type of contentData received by a server or client
    /// </summary>
    public static class DataDetector
    {
        /// <summary>
        /// Checks if a byte array is an image
        /// </summary>
        /// <param name="bytes">The byte array to check</param>
        /// <returns>Whether the byte array is an image</returns
        public static bool IsValidImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    Image.FromStream(memoryStream);
                }
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if an object is a list
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>Whether the object is a list</returns>
        public static bool IsList(object obj)
        {
            if (obj == null) return false;
            return obj is IList &&
                   obj.GetType().IsGenericType &&
                   obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        /// <summary>
        /// Checks if an object is a dictionary
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <returns>Whether the object is a dictionary</returns>
        public static bool IsDictionary(object obj)
        {
            if (obj == null) return false;
            return obj is IDictionary &&
                   obj.GetType().IsGenericType &&
                   obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }
    }
}
