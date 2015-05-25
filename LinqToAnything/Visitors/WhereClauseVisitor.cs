using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything.Results;
using System.Reflection;

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
                "Contains"

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

        protected override Expression VisitMember(MemberExpression node)
        {
            
            var result = base.VisitMember(node);
            _stack.Last().Parameters.Add(new Member
            {
                Name = node.Member.Name
            });
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

            var result = base.VisitMethodCall(node);

            var lastInserted = _stack.Last();
            _stack.RemoveAt(_stack.Count - 1);

            if (_stack.Count == 0)
            {
                lastInserted.Expression = node;
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
                lastInserted.Expression = node;
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