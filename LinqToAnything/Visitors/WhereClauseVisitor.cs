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
                var unary = node.Expression as UnaryExpression;
                
                object value = null;
                if (unary==null)
                {
                    var lambda = Expression.Lambda(node);
                    var lc = lambda.Compile();
                    value = lc.DynamicInvoke();
                    //result =  this.Visit(node.Expression);
                    _stack.Last().Parameters.Add(new Constant
                    {
                        Value = value
                    });
                    result = Expression.Constant(value);
                }
                else
                {
                    if (parameter == unary.Operand)
                    {
                        _stack.Last().Parameters.Add(new Member
                            {
                                Name = node.Member.Name
                            });
                    }
                    else
                    {
                        /*
                        if ((node.Member as PropertyInfo) != null)
                        {
                            if (!(node.Expression is MemberExpression))
                            {
                                return this.Visit(node.Expression);
                            }
                            var exp = (MemberExpression)node.Expression;
                            var constant = (ConstantExpression)exp.Expression;
                            var fieldInfoValue = ((FieldInfo)exp.Member).GetValue(constant.Value);
                            value = ((PropertyInfo)node.Member).GetValue(fieldInfoValue, null);

                        }
                        else if ((node.Member as FieldInfo) != null)
                        {
                            if (!(node.Expression is ConstantExpression))
                            {
                                return this.Visit(node.Expression);
                            }
                            var fieldInfo = node.Member as FieldInfo;
                            var constantExpression = node.Expression as ConstantExpression;
                            if (fieldInfo != null & constantExpression != null)
                            {
                                value = fieldInfo.GetValue(constantExpression.Value);
                            }
                        }*/
                        value = GetValue(node);
                        _stack.Last().Parameters.Add(new Constant
                        {
                            Value = value
                        });
                    }
                    
                    result = base.VisitMember(node);
                }
                
                
                

            }
            return result;
        }
        private object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

/*
        private void VisitMemberAccess(MemberExpression expression, MemberExpression left)
        {
            // To preserve Case between key/value pairs, we always want to use the LEFT side of the expression.
            // therefore, if left is null, then expression is actually left. 
            // Doing this ensures that our `key` matches between parameter names and database fields
            var key = left != null ? left.Member.Name : expression.Member.Name;

            // If the NodeType is a `Parameter`, we want to add the key as a DB Field name to our string collection
            // Otherwise, we want to add the key as a DB Parameter to our string collection
            if (expression.Expression.NodeType.ToString() == "Parameter")
            {
                _strings.Add(string.Format("[{0}]", key));
            }
            else
            {
                _strings.Add(string.Format("@{0}", key));

                // If the key is being added as a DB Parameter, then we have to also add the Parameter key/value pair to the collection
                // Because we're working off of Model Objects that should only contain Properties or Fields,
                // there should only be two options. PropertyInfo or FieldInfo... let's extract the VALUE accordingly
                var value = new object();
                if ((expression.Member as PropertyInfo) != null)
                {
                    var exp = (MemberExpression)expression.Expression;
                    var constant = (ConstantExpression)exp.Expression;
                    var fieldInfoValue = ((FieldInfo)exp.Member).GetValue(constant.Value);
                    value = ((PropertyInfo)expression.Member).GetValue(fieldInfoValue, null);

                }
                else if ((expression.Member as FieldInfo) != null)
                {
                    var fieldInfo = expression.Member as FieldInfo;
                    var constantExpression = expression.Expression as ConstantExpression;
                    if (fieldInfo != null & constantExpression != null)
                    {
                        value = fieldInfo.GetValue(constantExpression.Value);
                    }
                }
                else
                {
                    throw new InvalidMemberException();
                }

                // Add the Parameter Key/Value pair.
                Parameters.Add("@" + key, value);
            }
        }*/

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