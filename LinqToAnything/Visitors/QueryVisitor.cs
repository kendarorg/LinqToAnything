using System;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything.Results;

namespace LinqToAnything.Visitors
{
    public class QueryVisitor : System.Linq.Expressions.ExpressionVisitor
    {
        public QueryInfo QueryInfo { get; private set; }


        public QueryVisitor(QueryInfo queryInfo = null)
        {
            this.QueryInfo = queryInfo ?? new QueryInfo();
        }

        public override Expression Visit(Expression node)
        {
            return base.Visit(node);
        }

        // override ExpressionVisitor method
        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if ((m.Method.DeclaringType == typeof(Queryable)) || (m.Method.DeclaringType == typeof(Enumerable)))
            {
                switch (m.Method.Name)
                {
                    case ("Skip"):
                        {
                            Visit(m.Arguments[0]);

                            var countExpression = (ConstantExpression)(m.Arguments[1]);

                            QueryInfo.Skip = ((int)countExpression.Value);
                            return m;
                        }
                    case ("Take"):
                        {
                            Visit(m.Arguments[0]);

                            var countExpression = (ConstantExpression)(m.Arguments[1]);

                            QueryInfo.Take = ((int)countExpression.Value);
                            return m;
                        }
                    case ("Select"):
                        {
                            MethodCallExpression call = m;
                            LambdaExpression lambda = (LambdaExpression)ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                            Expression body = ExpressionUtils.RemoveQuotes(lambda.Body);
                            Select = new ExpressionUtils.SelectCallMatch
                            {
                                MethodCall = call,
                                Source = call.Arguments[0],
                                Lambda = lambda,
                                LambdaBody = body
                            };
                            break;
                        }
                    case ("OrderByDescending"):
                    case ("ThenByDescending"):
                        {
                            MethodCallExpression call = m;
                            var lambda = (LambdaExpression)ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                            var lambdaBody = (MemberExpression)ExpressionUtils.RemoveQuotes(lambda.Body);
                            var ob = new OrderBy(lambdaBody.Member.Name, OrderBy.OrderByDirection.Desc);
                            ob.Expression = lambda.Body;
                            QueryInfo.OrderBys.Add(ob);
                            break;
                        }
                    case ("OrderBy"):
                    case("ThenBy"):
                        {
                            MethodCallExpression call = m;
                            var lambda = (LambdaExpression)ExpressionUtils.RemoveQuotes(call.Arguments[1]);
                            var lambdaBody = (MemberExpression)ExpressionUtils.RemoveQuotes(lambda.Body);
                            var ob = new OrderBy(lambdaBody.Member.Name, OrderBy.OrderByDirection.Asc);
                            ob.Expression = lambda.Body;
                            QueryInfo.OrderBys.Add(ob);
                            break;
                        }
                    case ("Max"):
                    case ("Min"):
                        throw new NotImplementedException();
                        break;
                    case ("Where"):
                        {
                            MethodCallExpression call = m;
                            var whereClause = call.Arguments[1];
                            var whereClauseVisitor = new WhereClauseVisitor();
                            whereClauseVisitor.Visit(whereClause);
                            QueryInfo.Clauses = QueryInfo.Clauses.Concat((whereClauseVisitor.Filters)).ToArray();
                            break;
                        }
                }

            }
            return m;
        }

        public ExpressionUtils.SelectCallMatch Select { get; set; }

        public Func<TIn, TOut> Transform<TIn, TOut>()
        {
            if (Select == null) return new Func<TIn, TOut>(i => (TOut)(object)i);
            return (Func<TIn, TOut>)Select.Lambda.Compile();
        }
    }

}