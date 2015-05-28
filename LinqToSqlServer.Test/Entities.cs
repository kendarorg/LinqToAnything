using System;
namespace LinqToSqlServer.Test
{
    public class SomeEntityVm
    {
        public string Name { get; set; }
    }

    public interface IEntity
    {
        Guid Id { get; set; }
    }

    public class SomeEntity : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public int OuterIndex { get; set; }
    }

    public class Projection
    {
        public string Item { get; set; }
    }

}

