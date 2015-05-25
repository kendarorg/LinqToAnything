using System.Collections.Generic;
using Linq2Anything.Results;

namespace Linq2Anything
{
    public delegate IEnumerable<T> DataQuery<out T>(QueryInfo info);
    public delegate int CountQuery(QueryInfo info);
}