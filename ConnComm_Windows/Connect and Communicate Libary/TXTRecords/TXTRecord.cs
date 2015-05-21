using System;

namespace ConnComm
{
    /// <summary>
    /// The class that contains an indivudal key and value pair representing a TXTRecord
    /// </summary>
    public class TXTRecord
    {
        private string _key;
        private string _value;

        #region Properties

        /// <summary>
        /// The string key of the TXTRecord
        /// </summary>
        public string Key {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// The string value of the TXTRecord
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        /// <summary>
        /// The default constructor for the TXTRecord object
        /// </summary>
        /// <param name="key">The string key of the TXTRecord</param>
        /// <param name="value">The string value of the TXTRecord</param>
        public TXTRecord(string key, string value)
        {
            _key = key;
            _value = value;
        }

        /// <summary>
        /// Use of the empty constructor is prevented
        /// </summary>
        private TXTRecord()
        {

        }

        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the TXTRecord in a readable format</returns>
        public override string ToString()
        {
            return "TXTRecord: key = " + _key + "; value = " + _value;
        }
    }
}
