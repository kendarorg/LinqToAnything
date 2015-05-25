using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace LinqToAnything.Results
{
    public class Where : Clause
    {
        public string Operator { get; set; }

        public override Clause Clone()
        {
            return new Where()
            {
                Operator = this.Operator,
                Expression = Expression,
                Parameters = Parameters.Select(p => p.Clone()).ToList()
            };
        }


        public override string ToString()
        {
            var pars = Parameters.Select(a => a.ToString()).ToArray();
            return string.Format("{0}.{1}({2})",pars.First(), Operator, string.Join(",", pars.Skip(1)));
        }
    }
}