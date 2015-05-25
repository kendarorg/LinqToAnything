using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToAnything.Results;
using NUnit.Framework;
using LinqToAnything;
using System.Linq.Dynamic;
namespace LinqToAnything.Tests
{
    [TestFixture]
    public class OrderTakeSkip
    {
        /*private static int Skipped;
        private static int? Taken;
        private static IEnumerable<SomeEntity> Data = Enumerable.Range(1, 10).Select(i => new SomeEntity { Index = i, Name = "Item " + i.ToString().PadLeft(2, '0') });*/

        [Test]
        public void CanSkipAndTake()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select,ds.Count);
            
            var items = pq.Skip(3).Take(2).ToArray();
            var query = ds.Query;

            Assert.AreEqual(3, query.Skip);
            Assert.AreEqual(2, query.Take);

        }

        [Test]
        public void CanDoOrderBy()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.OrderBy(p=>p.Index).ToArray();
            var query = ds.Query;

            Assert.AreEqual(1, query.OrderBys.Count());
            var ob = query.OrderBys.First();
            Assert.AreEqual("Index", ob.Name);
            Assert.AreEqual(OrderBy.OrderByDirection.Asc, ob.Direction);

        }

        [Test]
        public void CanDoOrderByDesc()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.OrderByDescending(p => p.Index).ToArray();
            var query = ds.Query;

            Assert.AreEqual(1, query.OrderBys.Count());
            var ob = query.OrderBys.First();
            Assert.AreEqual("Index", ob.Name);
            Assert.AreEqual(OrderBy.OrderByDirection.Desc, ob.Direction);

        }

        [Test]
        public void CanDoOrderByMulti()
        {
            var ds = new DataSource<SomeEntity>();

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(ds.Select, ds.Count);

            var items = pq.OrderByDescending(p => p.Index).ThenBy(p=>p.Name).ToArray();
            var query = ds.Query;

            Assert.AreEqual(2, query.OrderBys.Count());
            var ob = query.OrderBys.First();
            Assert.AreEqual("Index", ob.Name);
            Assert.AreEqual(OrderBy.OrderByDirection.Desc, ob.Direction);

        }

        
        #if IGNORE
        [Test]
        public void CanDoACountWithAFilter()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.Contains("07"));
            var count = items.Count();

            Assert.AreEqual(1, count);
        }

        [Test]
        public void CanDoACountWithANullComparison()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name != null);
            var count = items.Count();

            Assert.AreEqual(10, count);
        }


        [Test]
        public void CanDoACountWithNoIllEffect()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);

            var count = pq.Count();

            Assert.AreEqual(10, count);
        }

        [Test]
        public void CanDoATakeWithNoIllEffectOnOtherQueries()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var somethingElse = pq.Take(5);
            Assert.AreEqual(5, somethingElse.Count());

            Assert.AreEqual(10, pq.Count());
        }


        [Test]
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





        [Test]
        public void CanWorkWithoutQuery()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);

            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);

            var items = pq.ToArray();

            Assert.AreEqual(0, Skipped);
            Assert.AreEqual(null, Taken);

            Assert.AreEqual("Item 01,Item 02", string.Join(",", items.Take(2).Select(i => i.Name)));

        }

        [Test]
        public void CanHandleAProjection()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Select(s => new Projection() { Item = s.Name });
            Assert.AreEqual("Item 01", items.ToArray().First().Item);
        }

        [Test]
        public void CanHandleAMethodCallWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.Contains("07"));
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [Test]
        public void CanHandleAnOperatorWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07");
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }


        [Test]
        public void CanHandleAnOperatorWhereClauseAgainstAVariable()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var variable = "Item 07";
            var items = pq.Where(s => s.Name == variable);
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }
        [Test]
        public void CanHandleASecondWhereClauseAfterACount()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07");
            var count = items.Count();
            var items2 = items.Where(s => s.Name != "Item 07");
            Assert.AreEqual(0, items2.ToArray().Length);
            
        }


        [Test]
        public void CanHandleAnOperatorWhereClauseOnAValueType()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Index != 0 && s.Index == 7);
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }



        [Test]
        public void CanHandleAnAndAlsoWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07" && s.Name == "Item 07");
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [Test]
        public void CanHandleAnOrElseWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name == "Item 07" || s.Name == "Item 08");
            Assert.AreEqual("Item 07", items.ToArray().First().Name);
            Assert.AreEqual("Item 08", items.ToArray().Skip(1).First().Name);
        }


        [Test]
        public void CanHandleAnEndsWithMethodCallWhereClause()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Where(s => s.Name.EndsWith("07"));
            Assert.AreEqual("Item 07", items.ToArray().Single().Name);
        }

        [Test]
        public void CanHandleASkipATakeAndAProjection()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 02", items.ToArray().First().Item);
        }
        [Test]
        public void CanHandleAProjectionASkipAndATake()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) => SomeDataSource(info);
            IQueryable<SomeEntity> pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            var items = pq.Skip(1).Take(1).Select(s => new Projection { Item = s.Name }).ToArray();
            Assert.AreEqual(1, Skipped);
            Assert.AreEqual(1, Taken);

            Assert.AreEqual("Item 02", items.ToArray().First().Item);
        }

        [Test]
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


        [Test]
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

        [Test, Ignore("Not implemented")]
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


        [Test]
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

        [Test]
        public void CanHandleAProjectionASkipAndAnOrderByAsc()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) =>
            {
                Assert.AreEqual(OrderBy.OrderByDirection.Asc, info.OrderBy.Direction);
                return Enumerable.Empty<SomeEntity>();
            };
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource);
            pq.OrderBy(e => e.Name).ToArray();
        }
        [Test]
        public void CanDoAnOptimizedCount()
        {
            DataQuery<SomeEntity> getPageFromDataSource = (info) =>
            {
                throw new NotImplementedException();
            };
            var pq = new DelegateQueryable<SomeEntity>(getPageFromDataSource, qi => 15);
            Assert.AreEqual(15, pq.Count(x => x.Index > 1));
        }
        [Test]
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
            if (qi.OrderBy != null)
            {
                var clause = qi.OrderBy.Name;
                if (qi.OrderBy.Direction == OrderBy.OrderByDirection.Desc) clause += " descending";
                query = query.OrderBy(clause);
            }

            foreach (var clause in qi.Clauses)
            {
                var where = clause as Where;
                if (where != null)
                {
                    if (where.Operator == "Contains" || where.Operator == "EndsWith")
                    {
                        query = query.Where(where.PropertyName + "." + clause.Operator + "(@0)", where.Value);
                    }
                    if (clause.Operator == "Equal")
                    {
                        query = query.Where(where.PropertyName + " == @0", where.Value);
                    }

                    if (clause.Operator == "NotEqual")
                    {
                        query = query.Where(where.PropertyName + " != @0", where.Value);
                    }


                }
                
                if (clause.Operator == "OrElse")
                {
                    query = query.Where((Expression<Func<SomeEntity, bool>>) ((Or)clause).Expression);
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
            if (qi.OrderBy != null)
            {
                var clause = qi.OrderBy.Name;
                if (qi.OrderBy.Direction == OrderBy.OrderByDirection.Desc) clause += " descending";
                query = query.OrderBy(clause);
            }

            foreach (var clause in qi.Clauses)
            {
                query = query.Where((Expression<Func<SomeEntity, bool>>) clause.Expression);

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
        #endif

    }
}
