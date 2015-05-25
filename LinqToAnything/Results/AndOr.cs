using System.Collections.Generic;
using System.Linq;

namespace LinqToAnything.Results
{
    public class AndOr : Clause
    {
        public string Operator { get; set; }

        public override Clause Clone()
        {
            return new AndOr()
            {
                Operator = Operator,
                Expression = Expression,
                Parameters = this.Parameters.Select(c => c.Clone()).ToList()
            };
        }

        public override string ToString()
        {
            return "(" + string.Join(" " + Operator + "  ", Parameters.Select(c => "(" + c.ToString() + ")").ToArray()) + ")";
        }
    }

    public class BinaryOperator : Clause
    {
        public string Operator { get; set; }

        public override Clause Clone()
        {
            return new BinaryOperator()
            {
                Operator = Operator,
                Expression = Expression,
                Parameters = this.Parameters.Select(c => c.Clone()).ToList()
            };
        }

        public override string ToString()
        {
            return  string.Join(" " + Operator + "  ", Parameters.Select(c => c.ToString() ).ToArray());
        }
    }
}