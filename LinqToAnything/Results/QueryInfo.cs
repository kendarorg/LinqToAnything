using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;

namespace LinqToAnything.Results
{
    /// <summary>
    /// an object representing a query. i.e.
    /// 
    ///     someQueryable
    ///         .Clause(x => x.Age >= 18)
    ///         .Clause(x => x.Name.Contains("John")
    ///         .OrderBy(x => x.Name).Skip(20).Take(10) 
    /// 
    /// would map to the values in the comments below
    /// </summary>


    public class QueryInfo
    {


        public QueryInfo()
        {
            OrderBys = new List<OrderBy>();
            Clauses = Enumerable.Empty<Clause>();
        }
        public int? Take { get; set; } //10
        public int Skip { get; set; } //20
        public List<OrderBy> OrderBys { get; set; } //LinqToAnything.OrderBy.Asc
        public IEnumerable<Clause> Clauses { get; set; } // an array containing two where objects

        public QueryInfo Clone()
        {
            return new QueryInfo()
            {
                Take = this.Take,
                Skip = this.Skip,
                OrderBys = OrderBys.Select(o => o.Clone()).ToList(),
                Clauses = this.Clauses.Select(c => c.Clone()).ToList()
            };
        }

        public override string ToString()
        {
            var res = "SELECT * ";
            if (Clauses.Count() > 0)
            {
                res += "WHERE ";
                
                foreach (var clause in Clauses)
                {
                    res += " " + clause.ToString() + " ";
                }
            }

            if (OrderBys.Count() > 0)
            {
                res += "ORDER BY ";
                res += string.Join(", ", OrderBys.Select(o => o.ToString()));
            }

            if (Skip > 0)
            {
                res += " SKIP " + Skip + " ";
            }
            if (Take != null && Take.Value > 0)
            {
                res += " TAKE " + Take + " ";
            }
            return res.Trim();
        }

        public IQueryable<T> ApplyTo<T>(IQueryable<T> q)
        {
            var qi = this;
            foreach (var clause in qi.Clauses)
            {
                q = q.Where((Expression<Func<T, bool>>)clause.Expression);
            }

            for (int index = 0; index < OrderBys.Count; index++)
            {
                var ob = OrderBys[index];
                var orderBy = ob.Name;
                if (ob.Direction == OrderBy.OrderByDirection.Desc)
                    orderBy += " descending";
                q = q.OrderBy(orderBy);
            }

            if (qi.Skip > 0) q = q.Skip(qi.Skip);

            if (qi.Take != null) q = q.Take(qi.Take.Value);

            return q;
        }
    }

    public static class QueryInfoExtension
    {
        public static T GetWhereClauseValue<T>(this QueryInfo qi, string propertyName, string @operator)
        {
            /*return qi.Clauses.OfType<Where>()
                .Where(c => c.PropertyName == propertyName && c.Operator == @operator)
                .Select(c => c.Value)
                .OfType<T>()
                .SingleOrDefault();*/
            throw new NotImplementedException();
        }
    }
}