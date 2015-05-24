using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Communicate.Common
{
    /// <summary>
    /// Representing the byte header for the type of contentData of a TCP packet
    /// </summary>
    public class ByteHeader
    {
        #region Private Variables

        private Int32 _byte1;
        private Int32 _byte2;

        private DataType _dataType;

        #endregion

        #region Properties

        /// <summary>
        /// The first byte value
        /// </summary>
        public Int32 Byte1
        {
            get { return Byte1; }
        }

        /// <summary>
        /// The second byte value 
        /// </summary>
        public Int32 Byte2
        {
            get { return Byte2; }
        }

        /// <summary>
        /// The type of contentData this header encodes for
        /// </summary>
        public DataType DataType
        {
            get { return _dataType; }
        }

        #endregion

        /// <summary>
        /// The default constructor for a byte header
        /// </summary>
        /// <param name="byte1">The first byte value</param>
        /// <param name="byte2">The second byte value</param>
        /// <param name="dataType">The type of contentData this header encodes for</param>
        public ByteHeader(Int32 byte1, Int32 byte2, DataType dataType)
        {
            _byte1 = byte1;
            _byte2 = byte2;
            _dataType = dataType;
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        public ByteHeader()
        {
            
        }

        /// <summary>
        /// The byte header for the string contentData type
        /// </summary>
        /// <returns>The byte header for the string contentData type</returns>
        public static ByteHeader StringByteHeader
        {
            get { return new ByteHeader(0x01, 0x01, DataType.String); }
        }

        /// <summary>
        /// The byte header for the image contentData type
        /// </summary>
        /// <returns>The byte header for the image contentData type</returns>
        public static ByteHeader ImageByteHeader
        {
            get { return new ByteHeader(0x02, 0x02, DataType.Image); }
        }

        /// <summary>
        /// The byte header for the file contentData type
        /// </summary>
        /// <returns>The byte header for the file contentData type</returns>
        public static ByteHeader FileByteHeader
        {
            get { return new ByteHeader(0x03, 0x03, DataType.File); }
        }

        /// <summary>
        /// The byte header for the JSON contentData type
        /// </summary>
        /// <returns>The byte header for the JSON contentData type</returns>
        public static ByteHeader JSONByteHeader
        {
            get { return new ByteHeader(0x04, 0x04, DataType.JSON); }
        }

        /// <summary>
        /// The byte header for the other contentData type
        /// </summary>
        /// <returns>The byte header for the other contentData type</returns>
        public static ByteHeader OtherByteHeader
        {
            get { return new ByteHeader(0x09, 0x09, DataType.Other); }
        }

        /// <summary>
        /// Creates a byte array from the byte header
        /// </summary>
        /// <returns>The byte array for this byte header</returns>
        public byte[] ToArray()
        {
            return new byte[] { (byte)_byte1, (byte)_byte2 };
        }

        #region Equality

        /// <summary>
        /// Overrides the equals function to compare exact byte headers
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>Whether this object and the object given are equal</returns>
        public override bool Equals(Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            ByteHeader p = obj as ByteHeader;
            if ((Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (_byte1 == p._byte1) && (_byte2 == p._byte2);
        }

        /// <summary>
        /// Overrides the equals function to compare exact byte headers
        /// </summary>
        /// <param name="obj">The object to compare with</param>
        /// <returns>Whether this object and the object given are equal</returns>
        public bool Equals(ByteHeader otherByteHeader)
        {
            // If parameter is null return false:
            if ((object)otherByteHeader == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (_byte1 == otherByteHeader._byte1) && (_byte2 == otherByteHeader._byte2);
        }

        /// <summary>
        /// Generates unique hash code based off the byte contentData of the header
        /// </summary>
        /// <returns>A unique hash code based off the byte contentData of the header</returns>
        public override int GetHashCode()
        {
            return _byte1 ^ _byte2;
        }

        #endregion

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the server in a readable format</returns>
        public override string ToString()
        {
            return "Byte Header: byte 1 = " + _byte1 + "; byte2 = " + _byte2 + "; data type = " + DataType.ToString();
        }
    }
}
