using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using VDS.RDF;
using static Serilog.Log;

namespace Libraries.QualityChecks
{
    /// <summary>
    /// Base class for all quality checks
    /// </summary>
    public abstract class QualityCheck : IQualityCheck
    {
        public const string PredicateBase = "resources://MeetsTheRequirementsOf/qualityCheckName";

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
        public abstract QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<object> parameters);

        public virtual string GetPredicate()
        {
            return PredicateBase.Replace("qualityCheckName", GetType().Name);
        }

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
                Error($"Cannot parse parameters of type {typeof(T)}: {e.GetDetails()}");
                throw;
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
                Error($"Failed to cancel {GetType().Name} quality check");
            }

            CancellationTokenSource.Dispose();
            CancellationTokenSource = new CancellationTokenSource();
            ParallelOptions = new ParallelOptions { CancellationToken = CancellationTokenSource.Token };
        }

        protected bool AreParametersValid(IEnumerable<object> parameters)
        {
            if (parameters == null || !parameters.Any())
            {
                Error($"Cannot run {GetType().Name} quality check with no parameters");
                return false;
            }

            //TODO maybe check what parameters are supported and if they are compatible?
            return true;
        }
    }
}
