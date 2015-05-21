using System;
using System.Collections;

namespace ConnComm
{
    /// <summary>
    /// The class that contains and serializes a list of TXTRecords to be included when a NetService is published
    /// </summary>
    public class TXTRecordList
    {
        /// <summary>
        /// The inner storage mechanism of TXTRecords
        /// </summary>
        private Hashtable innerHashtable;

        /// <summary>
        /// The default constructor for the TXTRecordList object
        /// </summary>
        public TXTRecordList()
        {
            innerHashtable = new Hashtable();
        }

        /// <summary>
        /// Adds a TXTRecord to the list of TXTRecords
        /// </summary>
        /// <param name="record">The TXTRecord object to add to the list of TXTRecords</param>
        public void AddTXTRecord(TXTRecord record)
        {
            innerHashtable.Add(record.Key, record.Value);
        }

        
        /// <summary>
        /// Adds a TXTRecord in the form of a key and value pair to the list of TXTRecords
        ///</summary>
        /// <param name="key">The key of the TXTRecord</param>
        /// <param name="value">The value of the TXTRecord</param>
        public void AddTXTRecord(string key, string value)
        {
            TXTRecord record = new TXTRecord(key, value);
            AddTXTRecord(record);
        }

        /// <summary>
        /// The method that returns a serialized version of a list of TXTRecords for use when publishing the server on the network
        /// </summary>
        /// <returns></returns>
        public Hashtable Serialize() {
            return innerHashtable;
        }

        /// <summary>
        /// This overrides the ToString method of this object to give more information for debugging use
        /// </summary>
        /// <returns>The information about the TXTRecords in a readable format</returns>
        public override string ToString() {
            return innerHashtable.ToString();
        }
    }
}
