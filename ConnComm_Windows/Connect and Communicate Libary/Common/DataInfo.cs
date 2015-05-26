using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Common
{
    /// <summary>
    /// A class that contains all information about a data communication
    /// </summary>
    public class DataInfo
    {
        #region Private Variables

        private CommunicationDataType _dataType;

        private int _headerLength;
        private int _contentLength;
        private int _footerLength;

        /// <summary>
        /// The list of all byte headers
        /// </summary>
        private static List<ByteHeader> byteHeaders = new List<ByteHeader>
        {
            ByteHeader.StringByteHeader, ByteHeader.ImageByteHeader, ByteHeader.FileByteHeader, ByteHeader.JSONByteHeader, ByteHeader.OtherByteHeader
        };

        #endregion

        #region Properties

        /// <summary>
        /// The type of data to send
        /// </summary>
        public CommunicationDataType DataType
        {
            get { return _dataType; }
        }

        /// <summary>
        /// The size of the header to send
        /// </summary>
        public int HeaderLength
        {
            get { return _headerLength; }
        }

        /// <summary>
        /// The size of the data to send
        /// </summary>
        public int ContentLength
        {
            get { return _contentLength; }
        }

        /// <summary>
        /// The size of the footer to send
        /// </summary>
        public int FooterLength
        {
            get { return _footerLength; }
        }

        /// <summary>
        /// The size of the header to send
        /// </summary>
        public static int DataInfoSize
        {
            get { return 14; }
        }

        #endregion

        /// <summary>
        /// Constructs a data info header from the given information
        /// </summary>
        /// <param name="dataType">The type of data to send</param>
        /// <param name="content">The content to send</param>
        /// <param name="footer">The footer to send</param>
        public DataInfo(CommunicationDataType dataType, DataHeaderFooter header, DataContent content, DataHeaderFooter footer)
        {
            _dataType = dataType;
            if (header == null)
            {
                _headerLength = 0;
            }
            else
            {
                _headerLength = header.GetBytes().Length;
            }
            if (content == null)
            {
                _contentLength = 0;
            }
            else
            {
                _contentLength = content.GetBytes().Length;
            }
            if (footer == null)
            {
                _footerLength = 0;
            }
            else
            {
                _footerLength = footer.GetBytes().Length;
            }
        }

        /// <summary>
        /// Constructs a data info header from data
        /// </summary>
        /// <param name="headerData">The header info data to convert</param>
        public DataInfo(byte[] headerData)
        {
            byte[] typeBuffer = new byte[2] { headerData[0], headerData[1] };

            _dataType = DataTypeFromByteArray(typeBuffer);
            _headerLength = BitConverter.ToInt32(headerData, 2);
            _contentLength = BitConverter.ToInt32(headerData, 6);
            _footerLength = BitConverter.ToInt32(headerData, 10);
        }

        /// <summary>
        /// Generates a 14 byte custom byte array for the data info header
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] typeArray = ByteArrayFromDataType(_dataType);
            byte[] headerLengthArray = BitConverter.GetBytes(_headerLength);
            byte[] contentLengthArray = BitConverter.GetBytes(_contentLength);
            byte[] footerLengthArray = BitConverter.GetBytes(_footerLength);

            byte[] byteArray = new byte[DataInfoSize];

            Buffer.BlockCopy(typeArray, 0, byteArray, 0, typeArray.Length);
            Buffer.BlockCopy(headerLengthArray, 0, byteArray, typeArray.Length, typeArray.Length);
            Buffer.BlockCopy(contentLengthArray, 0, byteArray, typeArray.Length + headerLengthArray.Length, contentLengthArray.Length);
            Buffer.BlockCopy(footerLengthArray, 0, byteArray, typeArray.Length + headerLengthArray.Length + contentLengthArray.Length, footerLengthArray.Length);

            return byteArray;
        }

        /// <summary>
        /// Converts the type of data to a byte array to include in the 
        /// </summary>
        /// <param name="dataType">The type of data to be converted</param>
        /// <returns>A byte array representing the type of data</returns>
        private static byte[] ByteArrayFromDataType(CommunicationDataType dataType)
        {
            ByteHeader byteHeader = ByteHeader.OtherByteHeader;
            if (dataType == CommunicationDataType.String)
            {
                byteHeader = ByteHeader.StringByteHeader;
            }
            else if (dataType == CommunicationDataType.Image)
            {
                byteHeader = ByteHeader.ImageByteHeader;
            }
            else if (dataType == CommunicationDataType.File)
            {
                byteHeader = ByteHeader.FileByteHeader;
            }
            else if (dataType == CommunicationDataType.JSON)
            {
                byteHeader = ByteHeader.JSONByteHeader;
            }

            return byteHeader.ToArray();
        }

        /// <summary>
        /// Converts a byte array to a representation of the type of data received
        /// </summary>
        /// <param name="bytes">The header byte array containing bit information about the type of data</param>
        /// <returns>The type of data based on the header byte array provided</returns>
        public static CommunicationDataType DataTypeFromByteArray(byte[] bytes)
        {
            ByteHeader actualByteHeader = ByteHeader.OtherByteHeader;
            if (bytes != null)
            {
                if (bytes.Length >= 2)
                {
                    ByteHeader byteHeader = new ByteHeader(bytes[0], bytes[1], CommunicationDataType.Other);
                    foreach (ByteHeader standardByteHeader in byteHeaders)
                    {
                        if (byteHeader.Equals(standardByteHeader))
                        {
                            actualByteHeader = standardByteHeader;
                            break;
                        }
                    }
                }
            }
            return actualByteHeader.DataType;
        }

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the data info header in a readable format</returns>
        public override string ToString()
        {
            return "Data Info: data type = " + _dataType + "; header length = " + _headerLength.ToString() + "; content length = " + _contentLength.ToString() + "; footer length = " + _footerLength.ToString();
        }
    }
}
