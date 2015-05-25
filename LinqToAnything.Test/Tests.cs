using LinqToAnything.Results;
using LinqToAnything.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqToAnything.Test
{
    [TestClass]
    public class OrderTakeSkip
    {
        private static int Skipped;
        private static int? Taken;
        private static IEnumerable<SomeEntity> Data = Enumerable.Range(1, 10).Select(i => new SomeEntity { Index = i, Name = "Item " + i.ToString().PadLeft(2, '0') });

        [TestMethod]
        public void CanDoACountWithAFilter()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.Contains("07"));
            var count = items.Count();

            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void CanDoACountWithANullComparison()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name != null);
            var count = items.Count();

            Assert.AreEqual(10, count);
        }


        [TestMethod]
        public void CanDoACountWithNoIllEffect()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);

            var count = pq.Count();

            Assert.AreEqual(10, count);
        }

        [TestMethod]
        public void CanDoATakeWithNoIllEffectOnOtherQueries()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var somethingElse = pq.Take(5);
            Assert.AreEqual(5, somethingElse.Count());

            Assert.AreEqual(10, pq.Count());
        }


        [TestMethod]
        public void CanDoASelectWithNoIllEffectOnOtherQueries()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource).Select(e => new SomeEntityVm()
            {
                Name = e.Name
            });
            Assert.AreEqual(5, pq.Take(5).Count());
            Assert.AreEqual(10, pq.Count());
        }





        [TestMethod]
        public void CanWorkWithoutQuery()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);

            var items = pq.ToArray();

            Assert.AreEqual(0, Skipped);
            Assert.AreEqual(null, Taken);

            Assert.AreEqual("Item 01,Item 02", string.Join(",", items.Take(2).Select(i => i.Name)));

        }


        [TestMethod]
        public void CanHandleAMethodCallWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.Contains("07"));
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [TestMethod]
        public void CanHandleAnOperatorWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07");
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }



        [TestMethod]
        public void CanHandleAnOperatorWhereClauseOnAValueType()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Index != 0 && s.Index == 7);
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }



        [TestMethod]
        public void CanHandleAnAndAlsoWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07" && s.Name == "Item 07");
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [TestMethod]
        public void CanHandleAnOrElseWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07" || s.Name == "Item 08");
            Assert.AreEqual("Item 07", items.ToArray().First().Name);
            Assert.AreEqual("Item 08", items.ToArray().Skip(1).First().Name);
        }



        [TestMethod]
        public void CanHandleASkipATakeAndAProjection()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 02", items.ToArray().First().Item);
        }
        [TestMethod]
        public void CanHandleAProjectionASkipAndATake()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 02", items.ToArray().First().Item);
        }

        [TestMethod]
        public void CanHandleAProjectionAndACount()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var someEntities = pq
                .Where(i => i.Name.Contains("07"));
            var projection = someEntities
                .Select(s => new SomeEntityVm() { Name = s.Name })
                .Where(p => p.Name == "Item 07");
            var itemCount = projection.Count();
            Assert.AreEqual(1, itemCount);
        }


        [TestMethod]
        public void CanHandleAProjection()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Select(s => new Projection() { Item = s.Name });
            Assert.AreEqual("Item 01", items.ToArray().First().Item);
        }



        [TestMethod]
        public void CanHandleAnOperatorWhereClauseAgainstAVariable()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var variable = "Item 07";
            var items = pq.Where(s => s.Name == variable);
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }


        [TestMethod]
        public void CanHandleASecondWhereClauseAfterACount()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07");
            var count = items.Count();
            var items2 = items.Where(s => s.Name != "Item 07");
            Assert.AreEqual(0, items2.ToArray().Length);

        }


        [TestMethod]
        public void CanHandleAnEndsWithMethodCallWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.EndsWith("07"));
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [TestMethod]
        public void CanHandleAProjectionAndACountAgainstIncompleteProvider()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => IncompleteDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var someEntities = pq
                .Where(i => i.Name.Contains("07"));
            var projection = someEntities
                .Select(s => new Projection { Item = s.Name });
            var itemCount = projection.Count();
            Assert.AreEqual(10, itemCount);
        }

        [TestMethod]
        [Ignore]
        public void CanHandleAProjectionAndACountAgainstLambdaProvider()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => LambdaDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var someEntities = pq;
            var projection = someEntities
                .Select(s => new Projection { Item = s.Name })
                .Where(i => i.Item.Contains("07"));
            ;
            var itemCount = projection.Count();
            Assert.AreEqual(1, itemCount);
        }


        [TestMethod]
        public void CanHandleAProjectionASkipAndAnOrderByDesc()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.OrderByDescending(e => e.Name).Skip(1).Take(1)
                .Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 09", items.ToArray().First().Item);
        }

        [TestMethod]
        public void CanHandleAProjectionASkipAndAnOrderByAsc()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) =>
            {
                Assert.AreEqual(OrderBy.OrderByDirection.Asc, info.OrderBys.First().Direction);
                return Enumerable.Empty<SomeEntity>();
            };
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            pq.OrderBy(e => e.Name).ToArray();
        }
        [TestMethod]
        public void CanDoAnOptimizedCount()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) =>
            {
                throw new NotImplementedException();
            };
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource, qi => 15);
            Assert.AreEqual(15, pq.Count(x => x.Index > 1));
        }
        [TestMethod]
        public void CanApplyAQueryInfo()
        {
            var queryable = Enumerable.Range(1, 100).Select(i => new SomeEntity() { Name = "User" + i, Index = i }).ToArray().AsQueryable();


            DataQuery<SomeEntity> getPageFromDataSource = (info) =>
            {
                return info.ApplyTo(queryable);
            };
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            Assert.AreEqual(90, pq.OrderByDescending(o => o.Index).Skip(10).Take(1).Single().Index);
        }


        // this method could call a sproc, or a webservice etc.
        static IEnumerable<SomeEntity> SomeDataSource(QueryInfo qi)
        {
            Skipped = qi.Skip;
            Taken = qi.Take;
            var query = Data.AsQueryable();
            if (qi.OrderBys.FirstOrDefault() != null)
            {
                var clause = qi.OrderBys.FirstOrDefault().Name;
                if (qi.OrderBys.FirstOrDefault().Direction == OrderBy.OrderByDirection.Desc) clause += " descending";
                query = query.OrderBy(clause);
            }

            foreach (var clause in qi.Clauses)
            {
                var where = clause as Where;
                var binary = clause as BinaryOperator;
                if (where != null)
                {
                    var par = where.Parameters.First() as Member;
                    var value = where.Parameters.Last() as Constant;
                    if (where != null && where.Operator == "Contains" || where.Operator == "EndsWith")
                    {
                        query = query.Where(par.Name + "." + where.Operator + "(@0)", value.Value);
                    }
                    else if (binary != null && binary.Operator == "Equal")
                    {
                        query = query.Where(par.Name + " == @0", value.Value);
                    }
                    else if (binary != null && binary.Operator == "NotEqual")
                    {
                        query = query.Where(par.Name + " != @0", value.Value);
                    }
                }

                if (binary != null && binary.Operator == "OrElse")
                {
                    query = query.Where((Expression<Func<SomeEntity, bool>>)((AndOr)clause).Expression);
                }

            }

            query = query.Skip(qi.Skip);
            if (qi.Take != null) query = query.Take(qi.Take.Value);
            return query.ToArray();
        }

        static IEnumerable<SomeEntity> LambdaDataSource(QueryInfo qi)
        {
            Skipped = qi.Skip;
            Taken = qi.Take;
            var query = Data.AsQueryable();
            if (qi.OrderBys.FirstOrDefault() != null)
            {
                var clause = qi.OrderBys.FirstOrDefault().Name;
                if (qi.OrderBys.FirstOrDefault().Direction == OrderBy.OrderByDirection.Desc) clause += " descending";
                query = query.OrderBy(clause);
            }

            foreach (var clause in qi.Clauses)
            {
                query = query.Where((Expression<Func<SomeEntity, bool>>)clause.Expression);

            }

            query = query.Skip(qi.Skip);
            if (qi.Take != null) query = query.Take(qi.Take.Value);
            return query.ToArray();
        }

        static IEnumerable<SomeEntity> IncompleteDataSource(QueryInfo qi)
        {
            var query = Data.AsQueryable();
            return query.ToArray();
        }

    }
}
