using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LinqToAnything;
using LinqToAnything.Visitors;

namespace LinqToSqlServer
{
    public interface IResultContainer 
    {
        ParserResult Result { get; }
    }

    public class SqlServerQueryable<T> : IOrderedQueryable<T>,IResultContainer
    {
        private readonly bool _fake;
        private readonly QueryVisitor _queryVisitor;
        private  SqlServerQueryProvider<T> _provider;
        private readonly Expression _expression;
        private  string _table;
        private SqlConnection _connection;
        

        public ParserResult Result
        {
            get
            {
                return _provider.Result;
            }
        }

        public void Initialize(string table, SqlConnection connection,bool fake=false)
        {
            _table = table;

            _connection = connection;
            _provider = new SqlServerQueryProvider<T>(_table, connection, fake);
        }

       public SqlServerQueryable(string table,SqlConnection connection,bool fake = false)
        {
            _table = table;
           _fake = fake;
           _connection = connection;
           _provider = new SqlServerQueryProvider<T>(_table,connection, _fake);
            _expression = Expression.Constant(this);
        }

       internal SqlServerQueryable(string table, SqlConnection connection, bool fake, QueryVisitor queryVisitor = null)
       {
           _table = table;
           _fake = fake;
            _queryVisitor = queryVisitor;
            _connection = connection;
            _provider = new SqlServerQueryProvider<T>(_table, connection, _fake, _queryVisitor);
            _expression = Expression.Constant(this);
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
            return  _provider.GetEnumerable<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
