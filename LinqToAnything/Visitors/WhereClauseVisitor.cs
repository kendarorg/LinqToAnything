using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything.Results;
using System.Reflection;
using System.Collections;

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
                "Contains",
                "EndsWith",
                "StartsWith"

            }.Any(f => f == mi.Name) && mi.ReturnType == typeof(bool);
        }

        public List<Clause> _stack = new List<Clause>();

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return base.VisitUnary(node);
        }


        protected override Expression VisitParameter(ParameterExpression node)
        {
            //e
            return base.VisitParameter(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            var result = base.VisitInvocation(node);
            throw new NotImplementedException();
            return result;
        }


        protected override Expression VisitConstant(ConstantExpression node)
        {
            var result = base.VisitConstant(node);
            var lastFilter = _stack.Last();
            lastFilter.Parameters.Add(new Constant
            {
                Value = node.Value
            });
            return result;
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            else if (expression is LambdaExpression)
            {
                var lambdaExpression = expression as LambdaExpression;
                if (lambdaExpression.Body is MemberExpression)
                {
                    return (MemberExpression)lambdaExpression.Body;
                }
                else if (lambdaExpression.Body is UnaryExpression)
                {
                    return ((MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand);
                }
            }
            return null;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Expression result;
            if (node.Expression.NodeType == ExpressionType.Parameter)
            {
                result = base.VisitMember(node);

                var lastValue = _stack.Last().Parameters.LastOrDefault() as Constant;
                //result = base.VisitMember(node);
                //var cnst = node.Expression as ConstantExpression;
                if (lastValue != null && lastValue.Value != null && lastValue.Value.GetType() != typeof(string) && typeof(IEnumerable).IsAssignableFrom(lastValue.Value.GetType()))
                {
                    _stack.Last().Parameters.Insert(0,new Member
                    {
                        Name = node.Member.Name
                    });
                }
                else
                {
                    _stack.Last().Parameters.Add(new Member
                    {
                        Name = node.Member.Name
                    });
                }
            }
            else
            {
                var lambda = Expression.Lambda(node);
                var lc = lambda.Compile();
                var value = lc.DynamicInvoke();
                //result =  this.Visit(node.Expression);
                _stack.Last().Parameters.Add(new Constant
                {
                    Value = value
                });
                result = Expression.Constant(value);
                /*result = null;
                object value = null;
                var container = node.Expression as ConstantExpression;
                var subm = node.Expression as MemberExpression;


                if (container != null)
                {
                    //Direct access
                    var member = node.Member;

                    if (member is FieldInfo)
                    {
                        value = ((FieldInfo) member).GetValue(container.Value);
                        result = Expression.Constant(value);
                    }
                    if (member is PropertyInfo)
                    {
                        value = ((PropertyInfo) member).GetValue(container.Value, null);
                        result = Expression.Constant(value);
                    }



                    _stack.Last().Parameters.Add(new Constant
                    {
                        Value = value
                    });
                }
                else
                {
                    result = this.Visit(subm);
                }
                */
                

            }
            return result;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (IsFilter(node.Method))
            {
                var filter = new Where
                {
                    Operator = node.Method.Name

                };
                _stack.Add(filter);
            }
            else
            {
                var mc = new Call
                {
                    Method = node.Method.Name
                };
                _stack.Add(mc);
            }
            var realExp = Expression.Lambda(node, parameter);

            var result = base.VisitMethodCall(node);

            var lastInserted = _stack.Last();
            _stack.RemoveAt(_stack.Count - 1);

            if (_stack.Count == 0)
            {
                lastInserted.Expression = realExp;
                _filters.Add(lastInserted);
            }
            else
            {

                var lastFilter = _stack.Last();
                lastFilter.Parameters.Add(lastInserted);
            }

            return result;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var realExp = Expression.Lambda(node, parameter);
            //return base.VisitBinary(node);
            if (node.NodeType == ExpressionType.AndAlso)
            {
                var mc = new AndOr
                {
                    Operator = "AND"
                };
                _stack.Add(mc);
                this.Visit(node.Left as BinaryExpression);
                this.Visit(node.Right as BinaryExpression);
            }
            else if (node.NodeType == ExpressionType.OrElse)
            {

                var mc = new AndOr
                {
                    Operator = "OR"
                };
                _stack.Add(mc);
                this.Visit(node.Left as BinaryExpression);
                this.Visit(node.Right as BinaryExpression);
            }
            else
            {
                var op = new BinaryOperator
                {
                    Operator = node.NodeType.ToString()
                };
                _stack.Add(op);
                this.Visit(node.Left);
                this.Visit(node.Right);
            }

            var lastInserted = _stack.Last();
            _stack.RemoveAt(_stack.Count - 1);
            if (_stack.Count == 0)
            {
                lastInserted.Expression = realExp; //node;
                _filters.Add(lastInserted);
            }
            else
            {
                var lastFilter = _stack.Last();
                lastFilter.Parameters.Add(lastInserted);
            }

            return node;
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