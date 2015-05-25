using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToAnything
{

    public class ParseTreeItem
    {
        public ParseTreeItem()
        {
            Values = new List<ParseTreeItem>();
        }
        public int Numerosity { get; set; }
        public string Operator { get; set; }

        public List<ParseTreeItem> Values { get; set; }  
    }
}
