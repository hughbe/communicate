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
    public static class DataSerializer
    {
        /// <summary>
        /// The list of all byte headers
        /// </summary>
        private static List<ByteHeader> byteHeaders = new List<ByteHeader>
        {
            ByteHeader.StringByteHeader, ByteHeader.ImageByteHeader, ByteHeader.FileByteHeader, ByteHeader.JSONByteHeader, ByteHeader.OtherByteHeader
        };

        /// <summary>
        /// Converts a byte array to an object
        /// </summary>
        /// <param name="bytes">The bytes to convert into an object</param>
        /// <returns>An object based on the contentData received</returns>
        public static object ObjectFromByteArray(byte[] bytes)
        {
            object toReturn = bytes;
            try
            {
                if (DataDetector.IsValidImage(bytes))
                {
                    toReturn = ByteArrayToImage(bytes);
                }
                else
                {
                    toReturn = Encoding.ASCII.GetString(bytes);
                }
            }
            catch { }
            return toReturn;
        }
        
        /// <summary>
        /// Converts a byte array to a representation of the type of contentData received
        /// </summary>
        /// <param name="bytes">The header byte array containing bit information about the type of contentData</param>
        /// <returns>The type of contentData based on the header byte array provided</returns>
        public static DataType DataTypeFromByteArray(byte[] bytes)
        {
            DataType dataType = DataType.Other;
            if (bytes != null)
            {
                if (bytes.Length >= 2)
                {
                    ByteHeader byteHeader = new ByteHeader(bytes[0], bytes[1], DataType.Other);
                    ByteHeader actualHeader = null;
                    foreach (ByteHeader standardByteHeader in byteHeaders)
                    {
                        if (byteHeader.Equals(standardByteHeader))
                        {
                            actualHeader = standardByteHeader;
                            break;
                        }
                    }
                    if (actualHeader != null)
                    {
                        dataType = actualHeader.DataType;
                    }
                    else
                    {
                        dataType = DataType.Image;
                    }
                }
            }
            return dataType;
        }

        /// <summary>
        /// Converts the type of contentData to a byte array to include in the 6 bit custom header file
        /// </summary>
        /// <param name="dataType">The type of contentData to be converted</param>
        /// <returns>A byte array representing the type of contentData</returns>
        public static byte[] ByteArrayFromDataType(DataType dataType)
        {
            ByteHeader byteHeader = ByteHeader.OtherByteHeader;
            if (dataType == DataType.String)
            {
                byteHeader = ByteHeader.StringByteHeader;
            }
            else if (dataType == DataType.Image)
            {
                byteHeader = ByteHeader.ImageByteHeader;
            }
            else if (dataType == DataType.File)
            {
                byteHeader = ByteHeader.FileByteHeader;
            }

            return byteHeader.ToArray();
        }

        /// <summary>
        /// Converts a byte array to a string
        /// </summary>
        /// <param name="bytes">The byte array to convert</param>
        /// <returns>A string created from the byte array</returns>
        public static string ByteArrayToString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        /// <summary>
        /// Converts a string to a byte array
        /// </summary>
        /// <param name="theString">The string to convert</param>
        /// <param name="encoding">The encoding of the string to convert</param>
        /// <returns>A byte array representing the string</returns>
        public static Collection<byte[]> StringToByteArray(string theString, Encoding encoding) 
        {
            if (encoding != null && theString != null)
            {
                byte[] bytes = encoding.GetBytes(theString);
                byte[] footer = FooterForByteArray(bytes, null, DataType.String);
                byte[] header = HeaderForByteArray(bytes, footer, DataType.String);
                return new Collection<byte[]>() { header, bytes, footer };
            }
            return new Collection<byte[]>();
        }

        /// <summary>
        /// Converts an image to a byte array
        /// </summary>
        /// <param name="image">The image to convert</param>
        /// <param name="name">The name of the image to convert</param>
        /// <returns>A byte array representing the image</returns>
        public static Collection<byte[]> ImageToByteArray(Image image, string name)
        {
            if (image != null)
            {
                ImageConverter converter = new ImageConverter();
                byte[] bytes = (byte[])converter.ConvertTo(image, typeof(byte[]));
                byte[] footer = FooterForByteArray(bytes, name, DataType.Image);
                byte[] header = HeaderForByteArray(bytes, footer, DataType.Image);
                return new Collection<byte[]>() { header, bytes, footer };
            }
            return new Collection<byte[]>();
        }

        /// <summary>
        /// Converts an byte array to an image
        /// </summary>
        /// <param name="bytes">The bytes to convert</param>
        /// <returns>An image created from the byte array</returns>
        public static Image ByteArrayToImage(byte[] bytes)
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
        /// Converts a file at a given path to a byte array
        /// </summary>
        /// <param name="filePath">The file to convert</param>
        /// <returns>A byte array representing the file</returns>
        public static Collection<byte[]> FileToByteArray(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            byte[] bytes = File.ReadAllBytes(filePath);
            byte[] footer = FooterForByteArray(bytes, fileName, DataType.File);
            byte[] header = HeaderForByteArray(bytes, footer, DataType.File);
            return new Collection<byte[]>() { header, bytes, footer };
        }

        /// <summary>
        /// Converts contentData to a byte array
        /// </summary>
        /// <param name="dataToSend">The contentData to convert</param>
        /// <returns>A byte array representing the contentData</returns>
        public static Collection<byte[]> DataToByteArray(byte[] data)
        {
            byte[] footer = FooterForByteArray(data, null, DataType.Other);
            byte[] header = HeaderForByteArray(data, footer, DataType.Other);
            return new Collection<byte[]>() { header, data, footer };
        }
        
        /// <summary>
        /// Generates the 6 bit custom header for a the byte array
        /// </summary>
        /// <param name="sourceArray">The byte array to generate the header for</param>
        /// <param name="dataTye">The type of contentData the byte array encodes</param>
        /// <returns>The byte array of the new custom header</returns>
        public static byte[] HeaderForByteArray(byte[] sourceArray, byte[] footerArray, DataType dataType)
        {
            if (sourceArray != null)
            {
                byte[] headerArray = ByteArrayFromDataType(dataType);
                byte[] lengthArray = BitConverter.GetBytes(sourceArray.Length);
                byte[] footerLengthArray = BitConverter.GetBytes(footerArray.Length);

                byte[] byteArray = new byte[headerArray.Length + lengthArray.Length + footerLengthArray.Length];

                Buffer.BlockCopy(headerArray, 0, byteArray, 0, headerArray.Length);
                Buffer.BlockCopy(lengthArray, 0, byteArray, headerArray.Length, lengthArray.Length);
                Buffer.BlockCopy(footerLengthArray, 0, byteArray, headerArray.Length + lengthArray.Length, footerLengthArray.Length);

                return byteArray;
            }
            return new byte[0];
        }

        /// <summary>
        /// Generates the 90 bit custom footer for a the byte array
        /// </summary>
        /// <param name="sourceArray">The byte array to generate the header for</param>
        /// <param name="dataTye">The type of contentData the byte array encodes</param>
        /// <returns>The byte array of the new custom header</returns>
        private static byte[] FooterForByteArray(byte[] sourceArray, string footerContent, DataType dataType)
        {
            if (sourceArray != null)
            {
                byte[] footerArray = new byte[0];
                if(dataType == DataType.Image || dataType == DataType.File) 
                {
                    if (footerContent != null)
                    {
                        footerArray = Encoding.ASCII.GetBytes(footerContent);
                    }
                }

                return footerArray;
            }
            return new byte[0];
        }
    }
}
