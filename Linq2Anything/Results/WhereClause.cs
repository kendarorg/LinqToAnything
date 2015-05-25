using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linq2Anything.Results
{
    public class WhereClause
    {
        public WhereClause()
        {
            Values = new List<WhereClause>();
        }
        public string Operator { get; set; }
        public int Numerosity { get; set; }
        public List<WhereClause> Values { get; set; } 
    }
}
