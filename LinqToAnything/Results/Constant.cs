using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToAnything.Results
{
    public class Constant : Clause
    {
        public object Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

        public override Clause Clone()
        {
            return new Constant
            {
                Expression = Expression,
                Value = Value
            };
        }
    }
}
