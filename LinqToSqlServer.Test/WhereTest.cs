using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using System;

namespace LinqToSqlServer.Test
{
    [TestClass]
    public class WhereTest
    {
        [TestMethod]
        public void All()
        {


            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST", null, true);

            pq.ToArray();
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST] ", result.Sql);
        }

        [TestMethod]
        public void WhereWithValue()
        {


            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST", null, true);

            pq.Where(e => e.Index == 2).ToArray();
            var result= ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE [Index]=@p_0",result.Sql);
            Assert.AreEqual(2,result.Parameters["p_0"]);
        }


        [TestMethod]
        public void WhereWithContainsSingleValue()
        {
            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST",null, true);

            var items = pq.Where(e => e.Name.Contains("test")).ToArray();
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE [Name] LIKE @p_0", result.Sql);
            Assert.AreEqual("%test%", result.Parameters["p_0"]);
        }


        [TestMethod]
        public void WhereWithToLower()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST",null, true);

            var items = pq.Where(e => e.Name.ToLower().Contains("test")).ToArray();
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE LOWER([Name]) LIKE @p_0", result.Sql);
            Assert.AreEqual("%test%", result.Parameters["p_0"]);
        }

        [TestMethod]
        public void WhereWithField()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST",null, true);

            var items = pq.Where(e => e.Index == e.OuterIndex).ToArray();
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE [Index]=[OuterIndex]", result.Sql);
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

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST",null, true);

            var items = pq.Where(e => e.Index == item.Value.SubValue).ToArray();
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE [Index]=@p_0", result.Sql);
            Assert.AreEqual(12, result.Parameters["p_0"]);
        }


        [TestMethod]
        public void AndCondition()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST",null, true);

            var items = pq.Where(e => e.Index == 2 && e.Name != "Test").ToArray(); 
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE (([Index]=@p_0) AND ([Name]<>@p_1))", result.Sql);
            Assert.AreEqual(2, result.Parameters["p_0"]);
            Assert.AreEqual("Test", result.Parameters["p_1"]);

        }


        [TestMethod]
        public void OrCondition()
        {
            

            IQueryable<SomeEntity> pq = new SqlServerQueryable<SomeEntity>("TEST",null, true);

            var items = pq.Where(e => e.Index == 2 || e.Name != "Test").ToArray();
            var result = ((SqlServerQueryable<SomeEntity>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE (([Index]=@p_0) OR ([Name]<>@p_1))", result.Sql);
            Assert.AreEqual(2, result.Parameters["p_0"]);
            Assert.AreEqual("Test", result.Parameters["p_1"]);
        }

        public void Delete(Guid id)
        {

            Delete<SomeEntity>(a => a.Id == id);
        }

        public void Delete<T>(Expression<Func<T, bool>> expr)
        {
            IQueryable<T> pq = new SqlServerQueryable<T>("TEST", null, true);
            pq.Where(expr).ToArray();
            var result = ((SqlServerQueryable<T>)pq).Result;
            Assert.AreEqual("SELECT * FROM [TEST]  WHERE [Id]=@p_0", result.Sql);
            Assert.AreEqual(Guid.Empty, result.Parameters["p_0"]);
        }

        [TestMethod]
        public void UnaryExpressions()
        {
            Delete(Guid.Empty);
        }
    }

}
