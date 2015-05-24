using System;
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
    }
}
