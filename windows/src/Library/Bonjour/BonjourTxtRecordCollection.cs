using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ZeroconfService;

namespace Communicate.Bonjour
{
    public class BonjourTxtRecordCollection : TxtRecordCollection
    {
        public BonjourTxtRecordCollection(byte[] data) : this(Deserialize(data))
        {
        }

        public BonjourTxtRecordCollection(Collection<TxtRecord> txtRecords) : base(txtRecords)
        {
        }

        public static Collection<TxtRecord> Deserialize(byte[] data)
        {
            var txtRecords = new Collection<TxtRecord>();

            foreach (KeyValuePair<string, byte[]> keyValuePair in NetService.DictionaryFromTXTRecordData(data))
            {
                var txtRecord = new TxtRecord(keyValuePair.Key, Encoding.ASCII.GetString(keyValuePair.Value));
                txtRecords.Add(txtRecord);
            }

            return txtRecords;
        }

        public override byte[] Serialize()
        {
            var records = TxtRecords.ToDictionary(record => record.Key, record => record.Value);
            return NetService.DataFromTXTRecordDictionary(records);
        }
    }
}
