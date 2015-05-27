using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything;
using LinqToAnything.Visitors;

namespace LinqToObject
{
    public class LinqToObjectQueryProvider<T> : IQueryProvider
    {
        private readonly LinqToObjectDataQuery<T> _linqToObjectDataQuery;
        private readonly LinqToObjectCountQuery _linqToObjectCountQuery;
        private readonly QueryVisitor _queryVisitor;


        public LinqToObjectQueryProvider(LinqToObjectDataQuery<T> linqToObjectDataQuery, LinqToObjectCountQuery linqToObjectCountQuery, QueryVisitor queryVisitor = null)
        {
            _linqToObjectDataQuery = linqToObjectDataQuery;
            this._linqToObjectCountQuery = linqToObjectCountQuery;
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
                LinqToObjectDataQuery<TElement> q = info => _linqToObjectDataQuery(info).Select(queryVisitor.Transform<T, TElement>());
                return new LinqToObjectQueryable<TElement>(q, _linqToObjectCountQuery, null, queryVisitor);
            }
            return new LinqToObjectQueryable<TElement>((LinqToObjectDataQuery<TElement>)((object)_linqToObjectDataQuery), _linqToObjectCountQuery, expression, queryVisitor);
 
        }


        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            var results = _linqToObjectDataQuery(queryVisitor.QueryInfo);
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
                return (TResult) (object) _linqToObjectCountQuery(queryVisitor.QueryInfo);
            }

            var allResult = GetEnumerable<TResult>().AsQueryable();
            var newExp = Expression.Call(methodCallExpression.Method, Expression.Constant(allResult));
            return allResult.Provider.Execute<TResult>(newExp);
        }
    }

    
}