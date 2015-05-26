using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;

namespace Communicate.Common
{
    /// <summary>
    /// A class to help conver a byte array to an object
    /// </summary>
    public static class DataSerializers
    {
        /// <summary>
        /// A class for converting strings to and from byte arrays
        /// </summary>
        public static class StringSerializer
        {
            /// <summary>
            /// Converts a byte array to a string
            /// </summary>
            /// <param name="bytes">The byte array to convert</param>
            /// <returns>A string created from the byte array</returns>
            public static string StringFromByteArray(byte[] bytes)
            {
                return Encoding.ASCII.GetString(bytes);
            }

            /// <summary>
            /// Converts a string of a given encoding to a byte array
            /// </summary>
            /// <param name="message">The string to encode</param>
            /// <param name="encoding">The encoding of the string</param>
            /// <returns>A byte array created from the string</returns>
            public static byte[] ByteArrayFromString(string message, Encoding encoding)
            {
                return encoding.GetBytes(message);
            }
        }

        /// <summary>
        /// A class for converting image to and from byte arrays
        /// </summary>
        public static class ImageSerializer
        {
            /// <summary>
            /// Converts an byte array to an image
            /// </summary>
            /// <param name="bytes">The bytes to convert</param>
            /// <returns>An image created from the byte array</returns>
            public static Image ImageFromByteArray(byte[] bytes)
            {
                if (bytes != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream(bytes))
                    {
                        return Image.FromStream(memoryStream);
                    }
                }
                return null;
            }

            /// <summary>
            /// Converts an image into a byte array
            /// </summary>
            /// <param name="image">The image to convert</param>
            /// <returns>A byte array created from the image</returns>
            public static byte[] ByteArrayFromImage(Image image)
            {
                byte[] bytes = (byte[])new ImageConverter().ConvertTo(image, typeof(byte[]));
                
                return bytes;
            }
        }


        /// <summary>
        /// A class for converting a file to and from byte arrays
        /// </summary>
        public static class FileSerializer
        {
            /// <summary>
            /// Converts a file into a byte array
            /// </summary>
            /// <param name="filePath">The path of the file to convert</param>
            /// <returns>A byte array created from the file</returns>
            public static byte[] ByteArrayFromFile(string filePath)
            {
                return File.ReadAllBytes(filePath);
            }
        }
    }
}
