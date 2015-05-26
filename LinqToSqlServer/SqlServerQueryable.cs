using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LinqToAnything;

namespace LinqToSqlServer
{
    public class SqlServerQueryable<T> : IOrderedQueryable<T>
    {
        private readonly SqlServerQueryProvider<T> _provider;
        private readonly Expression _expression;

        public SqlServerQueryable(SqlConnection connection)
        {

            _provider = new SqlServerQueryProvider<T>(connection);
            _expression = Expression.Constant(this);
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
