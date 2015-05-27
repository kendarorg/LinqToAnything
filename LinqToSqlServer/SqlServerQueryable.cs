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
    public class SqlServerQueryable<T> : IOrderedQueryable<T>
    {
        private readonly bool _fake;
        private readonly QueryVisitor _queryVisitor;
        private readonly SqlServerQueryProvider<T> _provider;
        private readonly Expression _expression;

       public SqlServerQueryable(SqlConnection connection,bool fake = false)
        {
           _fake = fake;
           _provider = new SqlServerQueryProvider<T>(connection,_fake);
            _expression = Expression.Constant(this);
        }

        internal SqlServerQueryable(SqlConnection connection, bool fake, QueryVisitor queryVisitor = null)
       {
           _fake = fake;
            _queryVisitor = queryVisitor;

            _provider = new SqlServerQueryProvider<T>(connection, _fake);
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

        public ParserResult Result
        {
            get { return _provider.Result; }
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
