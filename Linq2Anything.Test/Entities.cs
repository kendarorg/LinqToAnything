using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linq2Anything.Results;

namespace Linq2Anything.Test
{
    public class SomeEntityVm
    {
        public string Name { get; set; }
    }

    public class SomeEntity
    {
        public string Name { get; set; }
        public int Index { get; set; }
    }

    public class Projection
    {
        public string Item { get; set; }
    }

    public class DataSource<T>
    {
        public DataSource(int count = 10)
        {
            TotalCount = count;
        }

        public int TotalCount { get; set; }
        public QueryInfo Query { get; private set; }
        public IEnumerable<T> SomeDataSource(QueryInfo qi)
        {
            Query = qi;
            return new List<T>();
        }
    }
}

