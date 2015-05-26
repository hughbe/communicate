using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Communicate.Common
{
    /// <summary>
    /// A class that contains all information about the data of a data communication
    /// </summary>
    public class DataContent
    {
        #region Private Variables

        private byte[] _innerData;

        #endregion
        
        /// <summary>
        /// Constructs a data content object from a string and its encoding
        /// </summary>
        /// <param name="data">The string to convert into data</param>
        /// <param name="encoding">The encoding of the string to convert</param>
        public DataContent(string data, Encoding encoding)
        {
            _innerData = DataSerializers.StringSerializer.ByteArrayFromString(data, encoding);
        }

        /// <summary>
        /// Constructs a data content object from an image
        /// </summary>
        /// <param name="image">The image to convert into data</param>
        public DataContent(Image image)
        {
            _innerData = DataSerializers.ImageSerializer.ByteArrayFromImage(image);
        }

        /// <summary>
        /// Constructs a data content object from a file
        /// </summary>
        /// <param name="filePath">The path of the file to convert into data</param>
        public DataContent(string filePath)
        {
            _innerData = DataSerializers.FileSerializer.ByteArrayFromFile(filePath);
        }

        /// <summary>
        /// Constructs a data content object from a byte array
        /// </summary>
        /// <param name="data">The byte array to convert into data</param>
        public DataContent(byte[] data)
        {
            _innerData = data;
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        public DataContent()
        {
            _innerData = new byte[0];
        }

        /// <summary>
        /// Converts a byte header into 
        /// </summary>
        /// <returns>A byte array representing the content of this data</returns>
        public byte[] GetBytes()
        {
            return _innerData;
        }

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the data content in a readable format</returns>
        public override string ToString()
        {
            return "Data Content: length = " + _innerData.Length.ToString();
        }
    }
}
