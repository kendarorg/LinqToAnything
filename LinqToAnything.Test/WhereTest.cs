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
    public class WhereTest
    {
        [TestMethod]
        public void WhereWithValue()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == 2).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index Equal  2", ds.Query.ToString());
        }


        [TestMethod]
        public void WhereWithContainsSingleValue()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Name.Contains("test")).ToArray();
            Assert.AreEqual("SELECT * WHERE  Name.Contains(test)", ds.Query.ToString());
        }


        [TestMethod]
        public void WhereWithToLower()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Name.ToLower().Contains("test")).ToArray();
            Assert.AreEqual("SELECT * WHERE  Name.ToLower().Contains(test)", ds.Query.ToString());
        }

        [TestMethod]
        public void WhereWithField()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == e.OuterIndex).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index Equal  OuterIndex", ds.Query.ToString());
        }

        [TestMethod]
        public void WhereWithObjectParameter()
        {
            var ds = new DataSource<SomeEntity>();

            var item = new
            {
                Value = new
                {
                    SubValue = 12
                }
            };

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == item.Value.SubValue).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index Equal  12", ds.Query.ToString());
        }

        [TestMethod]
        public void AndCondition()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == 2 && e.Name != "Test").ToArray();
            Assert.AreEqual("SELECT * WHERE  ((Index Equal  2) AND  (Name NotEqual  Test))", ds.Query.ToString());

        }


        [TestMethod]
        public void OrCondition()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == 2 || e.Name != "Test").ToArray();
            Assert.AreEqual("SELECT * WHERE  ((Index Equal  2) OR  (Name NotEqual  Test))", ds.Query.ToString());
        }
    }
}
