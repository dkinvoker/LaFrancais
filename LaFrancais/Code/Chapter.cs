using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaFrancais.Code
{
    internal class Chapter
    {
        public required string Name { get; set; }

        public required Module[] Modules { get; set; }
    }
}
