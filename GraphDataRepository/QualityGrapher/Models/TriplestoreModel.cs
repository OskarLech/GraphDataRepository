using System.Collections.Generic;
using Libraries.Server;
using QualityGrapher.Utilities;

namespace QualityGrapher.Models
{
    public class TriplestoreModel
    {
        public string Name { get; set; }
        public IEnumerable<SupportedOperations> SupportedOperations { get; set; }

        private ITriplestoreClient _triplestoreClient;
    }
}
