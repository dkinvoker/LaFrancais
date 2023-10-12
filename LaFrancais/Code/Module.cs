using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaFrancais.Code
{
    internal sealed class Module
    {
        public required string Name { get; set; }

        public required QuizEntry[] Entries { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
