using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Linq2Anything.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void CanSkipAndTake()
        {
            var ds = new DataSource<SomeEntity>();
            var sq = new DelegateQueryable<SomeEntity>(ds.SomeDataSource);
            

            IQueryable<SomeEntity> pq = sq;
            
            var items = pq.Skip(3).Take(2).ToArray();
            var qi = ds.Query;

            Assert.AreEqual(3, qi.Skip);
            Assert.AreEqual(2, qi.Take);
        }
    }
}
