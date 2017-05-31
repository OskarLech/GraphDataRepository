using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using VDS.RDF;
using static Serilog.Log;

namespace GraphDataRepository.QualityChecks
{
    /// <summary>
    /// Base class for all quality checks
    /// </summary>
    public abstract class QualityCheck : IQualityCheck
    {
        protected CancellationTokenSource CancellationTokenSource;
        protected ParallelOptions ParallelOptions;

        private bool _isCheckInProgress;
        private readonly object _lock = new object();

        protected bool IsCheckInProgress
        {
            get
            {
                lock (_lock)
                {
                    return _isCheckInProgress;
                }
            }
            set
            {
                lock (_lock)
                {
                    _isCheckInProgress = value;
                }
            }
        }

        protected QualityCheck()
        {
            CancellationTokenSource = new CancellationTokenSource();
            ParallelOptions = new ParallelOptions { CancellationToken = CancellationTokenSource.Token };
        }

        public abstract QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters);
        public abstract QualityCheckReport CheckData(IEnumerable<Triple> triples, IEnumerable<object> parameters, IEnumerable<IGraph> graphs = null);
        public abstract void FixErrors(QualityCheckReport qualityCheckReport, string dataset, IEnumerable<int> errorsToFix);
        public abstract bool ImportParameters(IEnumerable<object> parameters);

        protected IEnumerable<T> ParseParameters<T> (IEnumerable<object> parameters)
        {
            try
            {
                return typeof(T).IsAssignableFrom(typeof(IConvertible)) 
                    ? parameters.Select(StaticMethods.ConvertTo<T>).ToList() 
                    : parameters.Select(parameter => (T) parameter).ToList();
            }
            catch (Exception e)
            {
                Logger.Error($"Cannot parse parameters of type {typeof(T)}: {e.GetDetails()}");
                return null;
            }
        }

        public void CancelCheck()
        {
            CancellationTokenSource.Cancel();

            var retries = 50;
            while (IsCheckInProgress && --retries >= 0)
            {
                Task.Delay(100).Wait();
            }

            if (retries <= 0)
            {
                Logger.Error($"Failed to cancel {GetType().Name} quality check");
            }

            CancellationTokenSource.Dispose();
            CancellationTokenSource = new CancellationTokenSource();
            ParallelOptions = new ParallelOptions { CancellationToken = CancellationTokenSource.Token };
        }

        public IEnumerable<string> GetParameters()
        {
            //from DB
            throw new NotImplementedException();
        }

        protected bool AreParametersSupported(IEnumerable<object> parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                Logger.Error($"Cannot run {GetType().Name} quality check with no parameters");
                return false;
            }

            //TODO
            return true;
        }
    }
}
