using System;
using System.Collections.Generic;
using Libraries.Server;
using static Libraries.Server.SupportedTriplestores;

namespace QualityGrapher.Models
{
    public class TriplestoreModel
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public IEnumerable<SupportedOperation> SupportedOperations { get; set; }
        public ITriplestoreClientQualityWrapper TriplestoreClientQualityWrapper { get; set; }
    }
}
