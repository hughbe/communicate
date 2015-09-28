using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Communicate
{
    public class TxtRecordCollection : IEnumerable<TxtRecord>
    {
        protected TxtRecordCollection(Collection<TxtRecord> txtRecords)
        {
            TxtRecords = txtRecords ?? new Collection<TxtRecord>();
        }

        public Collection<TxtRecord> TxtRecords { get; }

        public virtual byte[] Serialize() => new byte[0];
        
        public IEnumerator<TxtRecord> GetEnumerator() => TxtRecords.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => TxtRecords.GetEnumerator();
    }
}
