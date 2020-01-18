using System;

namespace Tests
{
    public class TestObject
    {
        public string Name { get; set; }
        public string ebDataType { get; set; }
        public DateTime Created { get; set; }

        public TestObject()
        {
            Created = DateTime.Now;
        }
    }
}