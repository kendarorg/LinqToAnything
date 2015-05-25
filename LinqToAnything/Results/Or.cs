using System.Collections.Generic;
using System.Linq;

namespace LinqToAnything.Results
{
    public class Or : Clause
    {
        public IEnumerable<Clause> Clauses { get; set; }

        public override Clause Clone()
        {
            return new Or()
            {
                Operator = this.Operator,
                Expression = Expression,
                Clauses = this.Clauses.Select(c => c.Clone())
            };
        }

        public override IEnumerable<string> PropertyNames
        {
            get { return Clauses.SelectMany(c => c.PropertyNames); }
        }

        public override string ToString()
        {
            return "(" + string.Join(") or ( ", Clauses.Select(c => c.ToString())) + ")";
        }
    }
}