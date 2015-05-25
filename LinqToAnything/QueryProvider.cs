using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything.Visitors;

namespace LinqToAnything
{
    public class QueryProvider<T> : IQueryProvider
    {
        private readonly DataQuery<T> _dataQuery;
        private readonly CountQuery _countQuery;
        private readonly QueryVisitor _queryVisitor;


        public QueryProvider(DataQuery<T> dataQuery, CountQuery countQuery, QueryVisitor queryVisitor = null)
        {
            _dataQuery = dataQuery;
            this._countQuery = countQuery;
            this._queryVisitor = queryVisitor ?? new QueryVisitor();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            queryVisitor.Visit(expression);
            if (typeof(TElement) != typeof(T))
            {
                DataQuery<TElement> q = info => _dataQuery(info).Select(queryVisitor.Transform<T, TElement>());
                return new DelegateQueryable<TElement>(q, _countQuery, null, queryVisitor);
            }
            return new DelegateQueryable<TElement>((DataQuery<TElement>)((object)_dataQuery), _countQuery, expression, queryVisitor);
 
        }


        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            var results = _dataQuery(queryVisitor.QueryInfo);
            //if (countQuery.Select != null)
            //{
            //    var projectionFunc = (Func<T, TResult>)countQuery.Select.Lambda.Compile();
            //    return results.Select(projectionFunc);
            //}
            return (IEnumerable<TResult>) results;
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute<T>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var methodCallExpression = (MethodCallExpression)expression;
            
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            queryVisitor.Visit(expression);
            if (methodCallExpression.Method.Name == "Count" && typeof(TResult) == typeof(int))
            {
                return (TResult) (object) _countQuery(queryVisitor.QueryInfo);
            }

            var array = _dataQuery(queryVisitor.QueryInfo).ToList();
            var data = array.AsQueryable();

            var newExp = Expression.Call(methodCallExpression.Method, Expression.Constant(data));
            return data.Provider.Execute<TResult>(newExp);
        }
    }

    
}