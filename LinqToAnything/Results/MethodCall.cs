using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToAnything.Results
{

    public class MethodCall : Clause
    {
        public string PropertyName { get; set; }

        public object UsableValue
        {
            get
            {
                var ce = Value as ConstantExpression;
                if (ce == null) return Value;
                return ce.Value;
            }
        }

        /// <summary>
        /// either a method name (e.g. Contains, StartsWith) or an operator name (op_Inequality,op_GreaterThan,op_GreaterThanOrEqual,op_LessThan,op_LessThanOrEqual,op_Multiply,op_Subtraction,op_Addition,op_Division,op_Modulus,op_BitwiseAnd,op_BitwiseOr,op_ExclusiveOr)
        /// </summary>
        public object Value { get; set; }


        public override Clause Clone()
        {
            return new Where()
            {
                Operator = this.Operator,
                PropertyName = this.PropertyName,
                Expression = Expression,
                Value = this.Value
            };
        }

        public override IEnumerable<string> PropertyNames
        {
            get { return new[] { PropertyName }; }
        }

        public override string ToString()
        {
            return " "+PropertyName+"."+ Operator;
        }
    }
}