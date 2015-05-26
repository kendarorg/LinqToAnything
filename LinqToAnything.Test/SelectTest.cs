using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToAnything.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToAnything.Tests
{
    [TestClass]
    public class SelectTest
    {
        [TestMethod]
        public void WhereWithValue()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == (e.Index + 2)).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index Equal  Index Add  2", ds.Query.ToString());
        }

    }
}
