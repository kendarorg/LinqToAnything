using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToAnything.Results;

namespace LinqToAnything.Visitors
{
    public class WhereClauseVisitor : ExpressionVisitor
    {
        private List<Clause> _filters = new List<Clause>();

        public IEnumerable<Clause> Filters
        {
            get { return _filters.AsReadOnly(); }
        }

        private Expression lambdaExpression;
        private dynamic parameter;

        private MethodCall _methodCall;

        public override Expression Visit(Expression node)
        {
            if (lambdaExpression == null)
            {
                lambdaExpression = node;
                parameter = ((dynamic)lambdaExpression).Operand.Parameters[0];
            }
            return base.Visit(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var filter = new Where();
            var memberExpression = node.Object as MemberExpression;

            if (memberExpression != null)
            {

                filter.PropertyName = memberExpression.Member.Name;
                filter.Expression = Expression.Lambda(node, parameter);
                filter.Operator = node.Method.Name;
                if (node.Arguments.Count == 1)
                {
                    filter.Value = node.Arguments[0];
                }
                else if (node.Arguments.Count == 0 && _methodCall != null)
                {
                    filter.Method = _methodCall;
                    _methodCall = null;

                }
                else
                {
                    throw new NotImplementedException();
                }
                /*else if (_methodCall != null)
                {
                    filter.Value = _methodCall;
                    //_methodCall.Value = filter;
                    _methodCall = null;
                }*/
                _filters.Add(filter);

                return base.VisitMethodCall(node);
            }

            var methodCallExpression = node.Object as MethodCallExpression;
            if (methodCallExpression != null)
            {
                var mex =(MemberExpression) methodCallExpression.Object;
                //var prop = (PropertyExpression) methodCallExpression.Object;
                var methodCall = new MethodCall();
                methodCall.Expression = Expression.Lambda(node, parameter);
                methodCall.Operator = node.Method.Name;
                methodCall.PropertyName = mex.Member.Name;
                

                _methodCall = methodCall;
                /*filter.Expression = Expression.Lambda(node, parameter);
                filter.Operator = node.Method.Name;*/
                return base.VisitMethodCall(node);
            }

            throw new Exception();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                this.Visit((BinaryExpression)node.Left);
                this.Visit((BinaryExpression)node.Right);
                return node;
            }
            else if (node.NodeType == ExpressionType.OrElse)
            {

                var filter = new Or { Operator = node.NodeType.ToString() };

                var whereVisitor = new WhereClauseVisitor();
                whereVisitor.lambdaExpression = this.lambdaExpression;

                whereVisitor.Visit(node.Left);
                whereVisitor.Visit(node.Right);
                filter.Clauses = whereVisitor.Filters;
                filter.Expression = Expression.Lambda(node, parameter);
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
                filter.Expression = Expression.Lambda(node, parameter);
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