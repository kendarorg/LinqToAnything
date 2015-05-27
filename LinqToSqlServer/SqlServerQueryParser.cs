using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToAnything.Results;

namespace LinqToSqlServer
{
    public class ParserResult
    {
        public string Sql { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
    public class SqlServerQueryParser
    {
        private readonly QueryInfo _queryInfo;
        private readonly string _methodName;

        public SqlServerQueryParser(QueryInfo queryInfo, string methodName = null)
        {
            _queryInfo = queryInfo;
            _methodName = methodName??"Where";
        }

        public ParserResult Parse()
        {

            throw new NotImplementedException();
        }
    }
}
