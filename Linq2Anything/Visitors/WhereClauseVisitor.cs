using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Linq2Anything.Results;

namespace Linq2Anything.Visitors
{
    public class WhereClauseVisitor : ExpressionVisitor
    {
        private WhereClause _filter = new WhereClause();

        /*public IEnumerable<Clause> Filters
        {
            get { return _filters.AsReadOnly(); }
        }*/

        private Expression _lambdaExpression;
        private dynamic _parameter;

        public override Expression Visit(Expression node)
        {
            if (_lambdaExpression == null)
            {
                _lambdaExpression = node;
                _parameter = ((dynamic) _lambdaExpression).Operand.Parameters[0];
            }
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new NotImplementedException();
            /*var filter = new Where();
            var memberExpression = ((System.Linq.Expressions.MemberExpression) node.Object);
            filter.PropertyName = memberExpression.Member.Name;
            filter.Expression = Expression.Lambda(node, _parameter);
            filter.Operator = node.Method.Name;
            filter.Value = node.Arguments[0];
            _filters.Add(filter);*/

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                this.Visit((BinaryExpression) node.Left);
                this.Visit((BinaryExpression) node.Right);
                return node;
            }
            else if (node.NodeType == ExpressionType.OrElse)
            {

                var filter = new Or {Operator = node.NodeType.ToString()};

                var whereVisitor = new WhereClauseVisitor();
                whereVisitor._lambdaExpression = this._lambdaExpression;

                whereVisitor.Visit(node.Left);
                whereVisitor.Visit(node.Right);
                filter.Clauses = whereVisitor.Filters;
                filter.Expression = Expression.Lambda(node, _parameter);
                _filters.Add(filter);
                return node;
            }
            else
            {
                var filter = new Where();
                var member = node.Left as MemberExpression;

                if (member == null)
                {
                    var unaryMember = node.Left as UnaryExpression;
                    if (unaryMember != null)
                    {
                        member = unaryMember.Operand as MemberExpression;
                    }
                }

                if (member != null)
                {
                    filter.PropertyName = member.Member.Name;
                }
                filter.Expression = Expression.Lambda(node, _parameter);
                filter.Operator = node.NodeType.ToString();
                filter.Value = GetValueFromExpression(node.Right);
                _filters.Add(filter);
                return node;
            }
        }
        private static object GetValueFromExpression(Expression node)
        {
            var member = node as MemberExpression;

            if (member == null)
            {
                var unaryMember = node as UnaryExpression;
                if (unaryMember != null)
                {
                    member = unaryMember.Operand as MemberExpression;
                }
            }

            if (member != null)
            {
                return Expression.Lambda(member).Compile().DynamicInvoke();
            }

            var constant = node as ConstantExpression;
            if (constant != null)
            {
                return constant.Value;
            }
            throw new NotImplementedException();
        }
    }
}