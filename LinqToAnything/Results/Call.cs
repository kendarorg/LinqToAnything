using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToAnything.Results
{
    
    
    public class Call:Clause
    {
        public string Method { get; set; }

        public override string ToString()
        {
            var pars = Parameters.Select(a=>a.ToString()).ToArray();
            return string.Format("{0}.{1}({2})",pars.First(), Method, string.Join(",", pars.Skip(1)));
        }

        public override Clause Clone()
        {
            return new Call
            {
                Method = Method,
                Parameters = Parameters.Select(p => p.Clone()).ToList()
            };
        }
    }
}
