using System.Collections.Generic;

namespace LinqToAnything.Tests
{
    public class SomeEntityVm
    {
        public string Name { get; set; }
    }

    public class SomeEntity
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int OuterIndex { get; set; }
    }

    public class Projection
    {
        public string Item { get; set; }
    }

}

