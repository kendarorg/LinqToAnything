using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToAnything.Results
{
    public abstract class Clause
    {
        public string Operator { get; set; }
        public Expression Expression { get; set; }
        public abstract Clause Clone();
        public abstract IEnumerable<string> PropertyNames { get;}
    }

}