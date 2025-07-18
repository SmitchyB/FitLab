using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitLab.Data
{
    public class Exercise
    {
        public Guid Guid { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> Type { get; set; } = new();
        public string MuscleGroup { get; set; } = string.Empty;
        public List<string> Equipment { get; set; } = new();
        public string Difficulty { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

}
