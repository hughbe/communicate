using System.Collections.Generic;

namespace Communicate
{
    public abstract class BaseTxtRecords
    {
        public List<TxtRecord> TxtRecords { get; protected set; } = new List<TxtRecord>();
        public abstract byte[] Serialize();
    }
}
