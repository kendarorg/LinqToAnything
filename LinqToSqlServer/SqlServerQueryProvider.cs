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
    public class SqlServerQueryProvider<T> : IQueryProvider, IResultContainer
    {
        private readonly bool _fake;
        private readonly string _table;
        private readonly SqlConnection _connection;
        private readonly QueryVisitor _queryVisitor;
        private ParserResult _result;
        public ParserResult Result
        {
            get
            {
                if (_result == null)
                {
                    if (_resultContainer != null)
                    {
                        return _resultContainer.Result;
                    }
                }
                return _result;
            }
        }

        public SqlServerQueryProvider(string table, SqlConnection connection, bool fake, QueryVisitor queryVisitor = null)
        {
            _table = table;
            _fake = fake;
            _queryVisitor = queryVisitor ?? new QueryVisitor();
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        private IResultContainer _resultContainer;

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            var queryVisitor = new QueryVisitor(_queryVisitor.QueryInfo.Clone());
            queryVisitor.Visit(expression);

            var parser = new SqlServerQueryParser(_table, queryVisitor.QueryInfo);
            _result = parser.Parse();

            var cnt = new SqlServerQueryable<TElement>(_table, _connection, _fake, queryVisitor);
            _resultContainer = cnt;
            return cnt;
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

                var parser = new SqlServerQueryParser(_table, queryVisitor.QueryInfo);
                _result = parser.Parse();
               
                if (_fake)
                {
                    return new List<TResult>();
                }
                return _connection.Query<TResult>(_result.Sql, _result.Parameters);
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
                    var parser = new SqlServerQueryParser(_table, queryVisitor.QueryInfo, methodCallExpression.Method.Name);
                    _result = parser.Parse();
                    if (_fake)
                    {
                        return default(TResult);
                    }
                    return _connection.ExecuteScalar<TResult>(Result.Sql, _result.Parameters);
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