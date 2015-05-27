using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinqToSqlServer.Test
{
    [TestClass]
    public class WhereTest
    {
        [TestMethod]
        public void WhereWithValue()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>(null, true);

            var items = pq.Where(e => e.Index == 2).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index Equal  2", ((SqlServerQueryable<SomeEntity>)pq).Result.Sql);
        }
        
#if notyet

        [TestMethod]
        public void WhereWithContainsSingleValue()
        {
            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>(null, true);

            var items = pq.Where(e => e.Name.Contains("test")).ToArray();
            Assert.AreEqual("SELECT * WHERE  Name.Contains(test)", ((SqlServerQueryable<SomeEntity>)pq).Result.Sql);
        }


        [TestMethod]
        public void WhereWithToLower()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>(null, true);

            var items = pq.Where(e => e.Name.ToLower().Contains("test")).ToArray();
            Assert.AreEqual("SELECT * WHERE  Name.ToLower().Contains(test)", ((SqlServerQueryable<SomeEntity>)pq).Result.Sql);
        }

        [TestMethod]
        public void WhereWithField()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>(null, true);

            var items = pq.Where(e => e.Index == e.OuterIndex).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index Equal  OuterIndex", ((SqlServerQueryable<SomeEntity>)pq).Result.Sql);
        }

        [TestMethod]
        public void WhereWithObjectParameter()
        {
            

            var item = new
            {
                Value = new
                {
                    SubValue = 12
                }
            };

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>(null, true);

            var items = pq.Where(e => e.Index == item.Value.SubValue).ToArray();
            Assert.AreEqual("SELECT * WHERE  Index Equal  12", ((SqlServerQueryable<SomeEntity>)pq).Result.Sql);
        }

        [TestMethod]
        public void AndCondition()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>(null, true);

            var items = pq.Where(e => e.Index == 2 && e.Name != "Test").ToArray();
            Assert.AreEqual("SELECT * WHERE  ((Index Equal  2) AND  (Name NotEqual  Test))", ((SqlServerQueryable<SomeEntity>)pq).Result.Sql);

        }


        [TestMethod]
        public void OrCondition()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>(null, true);

            var items = pq.Where(e => e.Index == 2 || e.Name != "Test").ToArray();
            Assert.AreEqual("SELECT * WHERE  ((Index Equal  2) OR  (Name NotEqual  Test))", ((SqlServerQueryable<SomeEntity>)pq).Result.Sql);
        }
#endif
    }

}
