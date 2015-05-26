using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything;
using LinqToAnything.Visitors;

namespace LinqToSqlServer
{
    public class SqlServerQueryProvider<T> : IQueryProvider
    {
        private readonly SqlConnection _connection;
        private readonly QueryVisitor _queryVisitor;


        public SqlServerQueryProvider(SqlConnection connection, QueryVisitor queryVisitor = null)
        {
            this._queryVisitor = queryVisitor ?? new QueryVisitor();
        }

        public SqlServerQueryProvider(SqlConnection connection)
        {
            _connection = connection;
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
                return new SqlServerQueryable<TElement>(_connection, queryVisitor);
            }
            return new SqlServerQueryable<TElement>(_connection, queryVisitor);
 
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