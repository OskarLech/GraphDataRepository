using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using VDS.RDF;

namespace GraphDataRepository.QualityChecks
{
    /// <summary>
    /// Base class for all quality checks
    /// </summary>
    public abstract class QualityCheck : IQualityCheck
    {
        protected CancellationTokenSource CancellationTokenSource;
        protected readonly ILog Log;
        
        protected QualityCheck(ILog log)
        {
            Log = log;
            CancellationTokenSource = new CancellationTokenSource();
        }

        public abstract QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<string> parameters = null);
        public abstract QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<IGraph> graphs = null, IEnumerable<string> parameters = null);
        public abstract void FixErrors(QualityCheckReport qualityCheckReport, string dataset, IEnumerable<int> errorsToFix);
        public abstract bool ImportParameters(IEnumerable<object> parameters);

        public void CancelCheck()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
            CancellationTokenSource = new CancellationTokenSource();
        }

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
