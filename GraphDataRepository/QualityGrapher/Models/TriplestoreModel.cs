using System.Collections.Generic;

namespace QualityGrapher.Models
{
    public class TriplestoreModel
    {
        public string Name { get; set; }
        public IEnumerable<string> SupportedOperations { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
