
using LinqToSqlServer;
using LinqToSqlServer.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToAnything.Test
{
    [TestClass]
    public class InClause
    {
        [TestMethod]
        public void WorkWithIn()
        {
            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST", null, true);
            var ids = new List<int>{1,4,8};

            var items = pq.Where(p => ids.Contains(p.Index)).ToArray();
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE [Index] IN @p_0", result.Sql);
            Assert.AreEqual(ids, result.Parameters["p_0"]);

        }
    }
}
