using System.Collections.Generic;
using LinqToAnything.Results;

namespace LinqToObject
{
    public delegate IEnumerable<T> LinqToObjectDataQuery<out T>(QueryInfo info);
    public delegate int LinqToObjectCountQuery(QueryInfo info);
}