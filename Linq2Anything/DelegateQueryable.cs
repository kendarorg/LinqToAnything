using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Linq2Anything.Visitors;

namespace Linq2Anything
{
    public class DelegateQueryable<T> : IOrderedQueryable<T>
    {
        QueryProvider<T> _provider;
        Expression _expression;

        public DelegateQueryable(DataQuery<T> dataQuery, CountQuery countQuery = null)
        {
        
            this._provider = new QueryProvider<T>(dataQuery, countQuery ?? (qi => dataQuery(qi).Count()));
            this._expression = Expression.Constant(this);
        }

        internal DelegateQueryable(DataQuery<T> dataQuery, CountQuery countQuery, Expression expression, QueryVisitor ev)
        {
            
            this._provider = new QueryProvider<T>(dataQuery, countQuery, ev);
            this._expression = expression ?? Expression.Constant(this);
            
        }

        Expression IQueryable.Expression
        {
            get { return this._expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this._provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this._provider.GetEnumerable<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }
}
