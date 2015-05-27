using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything;
using LinqToAnything.Visitors;

namespace LinqToObject
{
    public class LinqToObjectQueryable<T> : IOrderedQueryable<T>
    {
        readonly LinqToObjectQueryProvider<T> _provider;
        readonly Expression _expression;

        
        public LinqToObjectQueryable(LinqToObjectDataQuery<T> linqToObjectDataQuery, LinqToObjectCountQuery linqToObjectCountQuery = null)
        {
            _provider = new LinqToObjectQueryProvider<T>(linqToObjectDataQuery, linqToObjectCountQuery ?? (qi => linqToObjectDataQuery(qi).Count()));
            _expression = Expression.Constant(this);
        }

        internal LinqToObjectQueryable(LinqToObjectDataQuery<T> linqToObjectDataQuery, LinqToObjectCountQuery linqToObjectCountQuery, Expression expression, QueryVisitor ev)
        {
            
            _provider = new LinqToObjectQueryProvider<T>(linqToObjectDataQuery, linqToObjectCountQuery, ev);
            _expression = expression ?? Expression.Constant(this);
            
        }

        Expression IQueryable.Expression
        {
            get { return _expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _provider.GetEnumerable<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }
}
