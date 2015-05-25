using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToAnything.Results;
using NUnit.Framework;

namespace LinqToAnything.Tests
{
    [TestFixture]
    public class WhereTest
    {
        /*private static int Skipped;
        private static int? Taken;
        private static IEnumerable<SomeEntity> Data = Enumerable.Range(1, 10).Select(i => new SomeEntity { Index = i, Name = "Item " + i.ToString().PadLeft(2, '0') });*/

        [Test]
        public void WhereWithValue()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e=>e.Index==2).ToArray();
            var query = ds.Query;

            Assert.AreEqual(1,query.Clauses.Count());
            var cl = query.Clauses.First() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("Equal", cl.Operator);
            Assert.AreEqual(2, cl.UsableValue);
            Assert.AreEqual("Index", cl.PropertyNames.First());

        }


        [Test]
        public void WhereWithContainsSingleValue()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Name.Contains("test")).ToArray();
            var query = ds.Query;

            Assert.AreEqual(1, query.Clauses.Count());
            var cl = query.Clauses.First() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("Contains", cl.Operator);
            Assert.AreEqual("test", cl.UsableValue);
            Assert.AreEqual("Name", cl.PropertyNames.First());
        }


        [Test]
        public void WhereWithToLower()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Name.ToLower().Contains("test")).ToArray();
            var query = ds.Query;

            Assert.AreEqual(1, query.Clauses.Count());
            var cl = query.Clauses.First() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("Contains", cl.Operator);
            Assert.AreEqual("test", cl.UsableValue);
            Assert.AreEqual("Name", cl.PropertyNames.First());
        }

        [Test]
        [Ignore("Should it work?")]
        public void WhereWithField()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == e.OuterIndex).ToArray();
            var query = ds.Query;

            Assert.AreEqual(1, query.Clauses.Count());
            var cl = query.Clauses.First() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("Equal", cl.Operator);
            Assert.AreEqual(2, cl.UsableValue);
            Assert.AreEqual("Index", cl.PropertyNames.First());

        }

        [Test]
        public void AndCondition()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == 2 && e.Name!="Test").ToArray();
            var query = ds.Query;

            Assert.AreEqual(2, query.Clauses.Count());
            var cl = query.Clauses.First() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("Equal", cl.Operator);
            Assert.AreEqual(2, cl.UsableValue);
            Assert.AreEqual("Index", cl.PropertyNames.First());

            cl = query.Clauses.Last() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("NotEqual", cl.Operator);
            Assert.AreEqual("Test", cl.UsableValue);
            Assert.AreEqual("Name", cl.PropertyNames.First());

        }


        [Test]
        public void OrCondition()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.Where(e => e.Index == 2 || e.Name != "Test").ToArray();
            var query = ds.Query;

            Assert.AreEqual(1, query.Clauses.Count());
            var or = query.Clauses.First() as Or;

            Assert.IsNotNull(or);
            Assert.AreEqual("OrElse", or.Operator);
            Assert.AreEqual("Index", or.PropertyNames.First());

            var cl = or.Clauses.First() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("Equal", cl.Operator);
            Assert.AreEqual(2, cl.UsableValue);
            Assert.AreEqual("Index", cl.PropertyNames.First());

            cl = or.Clauses.Last() as Where;

            Assert.IsNotNull(cl);
            Assert.AreEqual("NotEqual", cl.Operator);
            Assert.AreEqual("Test", cl.UsableValue);
            Assert.AreEqual("Name", cl.PropertyNames.First());

        }
    }
}
