using LinqToAnything.Tests;
using LinqToObject;
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
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new LinqToObjectQueryable<SomeEntity>(ds.Select);
            var ids = new List<int>{1,4,8};

            var items = pq.Where(p => ids.Contains(p.Index)).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index.Contains(1,4,8)", ds.Query.ToString());

        }
    }
}
