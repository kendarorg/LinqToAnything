using System.Linq.Expressions;

namespace Linq2Anything.Visitors
{
    public class SwitchoutArgumentVisitor : ExpressionVisitor
    {
        private readonly object arg;

        public SwitchoutArgumentVisitor(object arg)
        {
            this.arg = arg;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return Expression.Constant(arg);
        }
    }
}