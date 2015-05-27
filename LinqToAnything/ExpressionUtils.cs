using LinqToAnything.Results;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToAnything
{
    public delegate IEnumerable<T> LinqToAnythingDataQuery<out T>(QueryInfo info);
    public delegate int LinqToAnythingCountQuery(QueryInfo info);

    public class DataSource<T>
    {
        public DataSource(int count = 10)
        {
            TotalCount = count;
        }

        public int TotalCount { get; set; }
        public QueryInfo Query { get; private set; }

        public IEnumerable<T> Select(QueryInfo qi)
        {
            Query = qi;
            return new List<T>();
        }

        public int Count(QueryInfo qi)
        {
            Query = qi;
            return -1;
        }
    }

    public static class ExpressionUtils
    {
        internal static Expression RemoveQuotes(Expression expr)
        {
            while (expr.NodeType == ExpressionType.Quote)
            {
                expr = ((UnaryExpression)expr).Operand;
            }

            return expr;
        }

        /// <summary>Match result for a SelectCall</summary>
        public class SelectCallMatch
        {
            /// <summary>The method call expression represented by this match.</summary>
            public MethodCallExpression MethodCall { get; set; }

            /// <summary>The expression on which the Select is being called.</summary>
            public Expression Source { get; set; }

            /// <summary>The lambda expression being executed by the Select.</summary>
            public LambdaExpression Lambda { get; set; }

            /// <summary>The body of the lambda expression.</summary>
            public Expression LambdaBody { get; set; }
        }
    }
}