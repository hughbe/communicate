using System;

namespace Demo.Bonjour
{
    [Serializable]
    public class SerializationTest
    {
        public string Property1 = "TEST!";
        public bool Property2 = true;
        public int Property3 = 1234;

        public override string ToString() => Property1 + "\t\t" + Property2 + "\t\t" + Property3;
    }
}