using System;
using System.Collections.Generic;
using log4net;
using VDS.RDF;

namespace GraphDataRepository.QualityChecks
{
    /// <summary>
    /// Base class for all quality checks
    /// </summary>
    public abstract class QualityCheck : IQualityCheck
    {
        protected readonly ILog Log;
        
        protected QualityCheck(ILog log)
        {
            Log = log;
        }

        public abstract QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<string> parameters = null);
        public abstract QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<IGraph> graphs = null, IEnumerable<string> parameters = null);
        public abstract void FixErrors(QualityCheckReport qualityCheckReport, string dataset, IEnumerable<int> errorsToFix);
        public abstract bool ImportParameters(IEnumerable<object> parameters);

        public IEnumerable<string> ListParameters()
        {
            //from DB
            throw new NotImplementedException();
        }

        protected QualityCheckReport GenerateQualityCheckReport()
        {
            throw new NotImplementedException();
        }
    }
}
