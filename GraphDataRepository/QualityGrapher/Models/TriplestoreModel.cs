using System.Collections.Generic;
using static Libraries.Server.SupportedTriplestores;

namespace QualityGrapher.Models
{
    public class TriplestoreModel
    {
        public string Name { get; set; }
        public IEnumerable<SupportedOperations> SupportedOperations { get; set; }
    }
}
