using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using ZeroconfService;

namespace Communicate.Bonjour
{
    public class BonjourTxtRecords : BaseTxtRecords, IEnumerable<TxtRecord>
    {
        public BonjourTxtRecords(byte[] data)
        {
            var txtRecordsDictionary = NetService.DictionaryFromTXTRecordData(data);
            TxtRecords = (from DictionaryEntry item in txtRecordsDictionary let value = Encoding.ASCII.GetString((byte[])item.Value) select new TxtRecord((string)item.Key, value)).ToList() ?? new List<TxtRecord>();
        }

        public BonjourTxtRecords(List<TxtRecord> txtRecords)
        {
            TxtRecords = txtRecords;
        }

        public override byte[] Serialize()
        {
            var records = TxtRecords.ToDictionary(record => record.Key, record => record.Value);
            return NetService.DataFromTXTRecordDictionary(records);
        }

        public IEnumerator<TxtRecord> GetEnumerator() => TxtRecords.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => TxtRecords.GetEnumerator();
    }
}
