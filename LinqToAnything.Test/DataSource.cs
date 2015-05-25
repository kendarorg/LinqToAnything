using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToAnything.Results;

namespace LinqToAnything.Tests
{
    public class DataSource<T>
    {
        public DataSource(int count = 10)
        {
            TotalCount = count;
        }

        public int TotalCount { get; set; }
        public QueryInfo Query { get; private set; }

        public IEnumerable<T> Select(QueryInfo qi)
        {
            Query = qi;
            return new List<T>();
        }

        public int Count(QueryInfo qi)
        {
            Query = qi;
            return -1;
        }
    }
}
