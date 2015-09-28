namespace Communicate
{
    public class TxtRecord
    {
        public TxtRecord(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }

        public override string ToString() => Key + " => " + Value;
    }
}