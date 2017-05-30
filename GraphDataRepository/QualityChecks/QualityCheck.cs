using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using VDS.RDF;

namespace GraphDataRepository.QualityChecks
{
    /// <summary>
    /// Base class for all quality checks
    /// </summary>
    public abstract class QualityCheck : IQualityCheck
    {
        protected CancellationTokenSource CancellationTokenSource;
        
        protected QualityCheck()
        {
            CancellationTokenSource = new CancellationTokenSource();
        }

        public abstract QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters = null);
        public abstract QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<IGraph> graphs = null, IEnumerable<object> parameters = null);
        public abstract void FixErrors(QualityCheckReport qualityCheckReport, string dataset, IEnumerable<int> errorsToFix);
        public abstract bool ImportParameters(IEnumerable<object> parameters);

        protected IEnumerable<T> ParseParameters<T> (T type, IEnumerable<object> parameters)
        {
            return parameters.Select(StaticMethods.ConvertTo<T>).ToList();
        }

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
