using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Collections;

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
            var lastValue = Parameters.Skip(1).FirstOrDefault() as Constant;

            if (lastValue != null && lastValue.Value != null && lastValue.Value.GetType() != typeof(string) && typeof(IEnumerable).IsAssignableFrom(lastValue.Value.GetType()))
            {
                var enume = lastValue.Value as IEnumerable;
                var result = new List<object>();
                foreach (var item in enume)
                {
                    result.Add(item);
                }
                return string.Format("{0}.{1}({2})", pars.First(), Operator, string.Join(",", result));
            }
            else
            {
                return string.Format("{0}.{1}({2})", pars.First(), Operator, string.Join(",", pars.Skip(1)));
            }

        }
    }
}