using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using LinqToAnything;
using LinqToAnything.Visitors;

namespace LinqToSqlServer
{
    public class SqlServerQueryProvider<T> : IQueryProvider
    {
        private readonly bool _fake;
        private readonly SqlConnection _connection;
        private readonly QueryVisitor _queryVisitor;
        public ParserResult Result { get; private set; }

        public SqlServerQueryProvider(SqlConnection connection,bool fake, QueryVisitor queryVisitor = null)
        {
            _fake = fake;
            _queryVisitor = queryVisitor ?? new QueryVisitor();
        }

        public SqlServerQueryProvider(SqlConnection connection, bool fake)
        {
            _fake = fake;
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
                return new SqlServerQueryable<TElement>(_connection,_fake, queryVisitor);
            }
            return new SqlServerQueryable<TElement>(_connection, _fake, queryVisitor);

        }


        public IEnumerable<TResult> GetEnumerable<TResult>()
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());

            var isNotOpen = false;
            if (!_fake)
            {
                isNotOpen = _connection.State != ConnectionState.Open;
            }
            try
            {
                if (isNotOpen && !_fake)
                {
                    _connection.Open();
                }
                var parser = new SqlServerQueryParser(queryVisitor.QueryInfo);
                Result = parser.Parse();
                if (_fake)
                {
                    return new List<TResult>();
                }
                return _connection.Query<TResult>(Result.Sql, Result.Parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                if (isNotOpen && !_fake)
                {
                    _connection.Close();
                }
            }
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute<T>(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var isNotOpen = false;
            if (!_fake)
            {
                isNotOpen = _connection.State != ConnectionState.Open;
            }
            try
            {
                if (isNotOpen && !_fake)
                {
                    _connection.Open();
                }

                var methodCallExpression = (MethodCallExpression)expression;

                var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
                queryVisitor.Visit(expression);

                if (methodCallExpression.Method.Name == "Count" && typeof(TResult) == typeof(int))
                {
                    var parser = new SqlServerQueryParser(queryVisitor.QueryInfo, methodCallExpression.Method.Name);
                    Result = parser.Parse();
                    if (_fake)
                    {
                        return default(TResult);
                    }
                    return _connection.ExecuteScalar<TResult>(Result.Sql, Result.Parameters);
                }

                var allResult = GetEnumerable<TResult>().AsQueryable();
                var newExp = Expression.Call(methodCallExpression.Method, Expression.Constant(allResult));
                return allResult.Provider.Execute<TResult>(newExp);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                if (isNotOpen && !_fake)
                {
                    _connection.Close();
                }
            }

        }
    }


}