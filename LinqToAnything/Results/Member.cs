using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToAnything.Results
{
    public class Member : Clause
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override Clause Clone()
        {
            return new Member
            {
                Name = Name
            };
        }
    }
}
