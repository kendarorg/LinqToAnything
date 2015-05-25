using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToAnything.Results
{
    public abstract class Clause
    {
        public Clause()
        {
            Parameters = new List<Clause>();
        }
        public Expression Expression { get; set; }
        public abstract Clause Clone();
        public List<Clause> Parameters { get; set; }

    }

}