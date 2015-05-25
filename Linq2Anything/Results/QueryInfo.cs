using System.Collections.Generic;

namespace Linq2Anything.Results
{
    //Will be QueryInstance
    public class QueryInfo
    {
        public QueryInfo()
        {
            OrderBys = new List<OrderBy>();
        }
        public int? Take { get; set; } 
        public int Skip { get; set; } 
        public List<OrderBy> OrderBys { get; set; }

        public void AddOrderBy(OrderBy ob)
        {
            OrderBys.Add(ob);
        }

        public QueryInfo Clone()
        {
            return new QueryInfo
            {
                Take = Take,
                Skip = Skip,
                OrderBys = new List<OrderBy>(OrderBys)
            };
        }
    }
}
