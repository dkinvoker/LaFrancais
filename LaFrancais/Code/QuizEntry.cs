using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaFrancais.Code
{
    internal sealed class QuizEntry
    {
        public required string Meaning { get; set; }
        public required string FrancaisSpelling { get; set; }
        public string? ImageLink { get; set; }
    }
}
