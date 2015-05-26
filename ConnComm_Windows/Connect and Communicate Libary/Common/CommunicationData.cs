using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Communicate.Common
{
	/// <summary>
	/// A class to hold all data sent when communicating with a server or client. It contains the header, content and footer byte arrays
	/// </summary>
    public class CommunicationData
    {
        #region Private Variables

        private DataInfo _dataInfo;
        private DataHeader _dataHeader;
        private DataContent _dataContent;
        private DataFooter _dataFooter;

        #endregion

        #region Properties

        /// <summary>
        /// The information about the data to send
        /// </summary>
        public DataInfo DataInfo
        {
            get { return _dataInfo; }
        }

        /// <summary>
        /// The header to send
        /// </summary>
        public DataHeader DataHeader
        {
            get { return _dataHeader; }
        }

        /// <summary>
        /// The content to send
        /// </summary>
        public DataContent DataContent
        {
            get { return _dataContent; }
        }

        /// <summary>
        /// The footer to send
        /// </summary>
        public DataFooter DataFooter
        {
            get { return _dataFooter; }
        }

        #endregion

        /// <summary>
        /// The default constructor for the communication data object
        /// </summary>
        /// <param name="header">The header to send</param>
        /// <param name="content">The content to send</param>
        /// <param name="footer">The footer to send</param>
        public CommunicationData(DataInfo dataInfo, DataHeader header, DataContent content, DataFooter footer)
        {
            if (header == null)
            {
                header = new DataHeader();
            }
            if (content == null)
            {
                content = new DataContent();
            }
            if (footer == null)
            {
                footer = new DataFooter();
            }
            _dataInfo = dataInfo;
            _dataHeader = header;
            _dataContent = content;
            _dataFooter = footer;
        }

        /// <summary>
        /// Converts a string to a communication data object
        /// </summary>
        /// <param name="theString">The string to convert</param>
        /// <param name="encoding">The encoding of the string to convert</param>
        /// <returns>A communication data object representing the string</returns>
        public static CommunicationData FromString(string theString, Encoding encoding)
        {
            if (encoding != null && theString != null)
            {
                DataHeader header = new DataHeader();
                DataContent content = new DataContent(theString, encoding);
                DataFooter footer = new DataFooter();
                DataInfo dataInfo = new DataInfo(CommunicationDataType.String, header, content, footer);

                return new CommunicationData(dataInfo, header, content, footer);
            }
            return null;
        }

        /// <summary>
        /// Converts an image to a communication data object
        /// </summary>
        /// <param name="image">The image to convert</param>
        /// <param name="name">The name of the image to convert</param>
        /// <returns>A communication data object representing the image</returns>
        public static CommunicationData FromImage(Image image, string name)
        {
            if (image != null)
            {
                DataHeader header = new DataHeader();
                header.FileName = name;
                DataContent content = new DataContent(image);
                DataFooter footer = new DataFooter();

                DataInfo dataInfo = new DataInfo(CommunicationDataType.Image, header, content, footer);

                return new CommunicationData(dataInfo, header, content, footer);
            }
            return null;
        }

        /// <summary>
        /// Converts a file at a given path to a communication data object
        /// </summary>
        /// <param name="filePath">The file to convert</param>
        /// <returns>A communication data object representing the file</returns>
        public static CommunicationData FromFile(string filePath, string name)
        {
            DataHeader header = new DataHeader();
            header.FileName = name;
            DataContent content = new DataContent(filePath);
            DataFooter footer = new DataFooter();

            DataInfo dataInfo = new DataInfo(CommunicationDataType.File, header, content, footer);

            return new CommunicationData(dataInfo, header, content, footer);
        }
        
        /// <summary>
        /// Converts a dictionary to a JSON string and a communication data object
        /// </summary>
        /// <param name="dictionary">The dictionary to convert</param>
        /// <returns>A communication data object representing the dictionary</returns>
        public static CommunicationData FromDictionary(Dictionary<object, object> dictionary)
        {
            byte[] JSONData = DataSerializers.StringSerializer.ByteArrayFromString(new JavaScriptSerializer().Serialize(dictionary), Encoding.ASCII);
            return CommunicationData.FromJSONData(JSONData, JSONObjectType.Dictionary);
        }

        /// <summary>
        /// Converts an array to a JSON string and a communication data object
        /// </summary>
        /// <param name="array">The array to convert</param>
        /// <returns>A communication data object representing the array</returns>
        public static CommunicationData FromArray(List<object> array)
        {
            byte[] JSONData = DataSerializers.StringSerializer.ByteArrayFromString(new JavaScriptSerializer().Serialize(array), Encoding.ASCII);
            return CommunicationData.FromJSONData(JSONData, JSONObjectType.Array);
        }

        public static CommunicationData FromJSONData(byte[] data, JSONObjectType objectType)
        {
            DataHeader header = new DataHeader();
            header.JsonObjectType = objectType;
            DataContent content = new DataContent(data);
            DataFooter footer = new DataFooter();

            DataInfo dataInfo = new DataInfo(CommunicationDataType.JSON, header, content, footer);

            return new CommunicationData(dataInfo, header, content, footer);
        }

        /// <summary>
        /// Converts a byte array to a communication data object
        /// </summary>
        /// <param name="data">The data to convert</param>
        /// <returns>A communication data object representing the data</returns>
        public static CommunicationData FromData(byte[] data)
        {
            DataHeader header = new DataHeader();
            DataContent content = new DataContent(data);
            DataFooter footer = new DataFooter();

            DataInfo dataInfo = new DataInfo(CommunicationDataType.Other, header, content, footer);

            return new CommunicationData(dataInfo, header, content, footer);
        }

        /// <summary>
        /// Converts received data into a string
        /// </summary>
        /// <returns>A string representation of data received</returns>
        public override string ToString()
        {
            return DataSerializers.StringSerializer.StringFromByteArray(_dataContent.GetBytes());
        }

        /// <summary>
        /// Converts received data into a string
        /// </summary>
        /// <returns>An image representation of data received</returns>
        public Image ToImage()
        {
            return DataSerializers.ImageSerializer.ImageFromByteArray(_dataContent.GetBytes());
        }

        /// <summary>
        /// Converts received data into a byte array
        /// </summary>
        /// <returns>A byte array represenation of data received</returns>
        public byte[] ToBytes()
        {
            return _dataContent.GetBytes();
        }

        /// <summary>
        /// Gets the type of data received
        /// </summary>
        /// <returns>The type of data received</returns>
        public CommunicationDataType GetDataType()
        {
            return _dataInfo.DataType;
        }
    }
}
