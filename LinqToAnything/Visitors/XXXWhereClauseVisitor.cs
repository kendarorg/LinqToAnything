using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using LinqToAnything.Results;
using System.Reflection;

namespace LinqToAnything.Visitors
{
    public class XXXWhereClauseVisitor : ExpressionVisitor
    {
        private List<Clause> _filters = new List<Clause>();

        public IEnumerable<Clause> Filters
        {
            get { return _filters.AsReadOnly(); }
        }

        private Expression lambdaExpression;
        private dynamic parameter;


        public override Expression Visit(Expression node)
        {
            if (lambdaExpression == null)
            {
                lambdaExpression = node;
                parameter = ((dynamic)lambdaExpression).Operand.Parameters[0];
            }
            return base.Visit(node);
        }

        private bool IsFilter(MethodInfo mi)
        {
            return new List<string>
            {
                "Contains"

            }.Any(f => f == mi.Name) && mi.ReturnType == typeof(bool);
        }

        private Where BuildFilter(MethodCallExpression node)
        {
            var filter = new Where();
            var memberExpression = node.Object as MemberExpression;
            filter.PropertyName = memberExpression.Member.Name;
            filter.Expression = Expression.Lambda(node, parameter);
            filter.Operator = node.Method.Name;
            if (node.Arguments.Count == 1)
            {
                //This only if ConstantExpression
                filter.Value = node.Arguments[0];
            }
            else
            {
                return filter;
            }
            return filter;
        }

        private Where BubbleUp(MethodCallExpression node)
        {
            var nodeMethod = node.Method;
            var methodName = node.Method.Name;
            var memberExpression = node.Object as MemberExpression;

            if (IsFilter(node.Method))
            {
                var filter = new Where();
                filter.Operator = methodName;

                var mcexp = node.Object as MethodCallExpression;
                
                if (mcexp != null)
                {
                    var result = BubbleUp(mcexp);
                    filter.Property = result;
                    //Load parameters

                    foreach (var arg in node.Arguments)
                    {
                        var constantExpr = arg as ConstantExpression;
                        if(constantExpr!=null)
                        {
                            var suf = new Where{
                                Value = constantExpr.Value
                            };
                            filter.Parameters.Add(suf);
                            continue;
                        }

                        var mex = arg as MethodCallExpression;
                        filter.Parameters.Add(BubbleUp(mex));
                    }

                    return filter;
                }
                throw new NotImplementedException();
            }
            else
            {
                return BuildFilter(node);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var filter = new Where();
            _filters.Add(BubbleUp(node));
            return //base.VisitMethodCall(node);
                Expression.Lambda(node,parameter);
#if NUP
            var nodeMethod = node.Method;
            if (IsFilter(node.Method))
            {
                //Is a filter expression
                filter.Operator = node.Method.Name;
                var mcexp = node.Object as MethodCallExpression;
                if (mcexp != null)
                {
                    var result = BubbleUp(mcexp);
                }
            }

            //This is the content of a Parameter
            var memberExpression = node.Object as MemberExpression;
            if (memberExpression != null)
            {

                filter.PropertyName = memberExpression.Member.Name;
                filter.Expression = Expression.Lambda(node, parameter);
                filter.Operator = node.Method.Name;
                if (node.Arguments.Count == 1)
                {
                    //This only if ConstantExpression
                    filter.Value = node.Arguments[0];
                }

                if (node.Method.ReturnType == typeof(bool))
                {
                    _filters.Add(filter);
                }
                return base.VisitMethodCall(node);
            }


            var methodCallExpression = node.Object as MethodCallExpression;
            if (methodCallExpression != null)
            {
                return this.VisitMethodCall(methodCallExpression);
                //var mex =(MemberExpression) methodCallExpression.Object;
                ////var prop = (PropertyExpression) methodCallExpression.Object;
                //var methodCall = new MethodCall();
                //methodCall.Expression = Expression.Lambda(node, parameter);
                //methodCall.Operator = node.Method.Name;
                //methodCall.PropertyName = mex.Member.Name;


                //_methodCall = methodCall;
                ///*filter.Expression = Expression.Lambda(node, parameter);
                //filter.Operator = node.Method.Name;*/
                //return base.VisitMethodCall(node);
            }

            throw new Exception();
#endif
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