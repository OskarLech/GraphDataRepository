using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Libraries.QualityChecks;
using Libraries.Server.BrightstarDb;
using VDS.RDF;
using VDS.RDF.Query;
using static Libraries.QualityChecks.QualityChecksData;
using static Serilog.Log;

namespace Libraries.Server
{
    /// <summary>
    /// Wraps ITriplestoreClient to enforce quality checks fulfillment
    /// </summary>
    public class TriplestoreClientQualityWrapper : ITriplestoreClientQualityWrapper
    {
        private const string TransactionAborted = "Transaction aborted";
        private readonly ITriplestoreClient _triplestoreClient;

        private CancellationTokenSource _cancellationTokenSource;
        private ParallelOptions _parallelOptions;

        public TriplestoreClientQualityWrapper(ITriplestoreClient triplestoreClient)
        {
            _triplestoreClient = triplestoreClient;
            _cancellationTokenSource = new CancellationTokenSource();
            _parallelOptions = new ParallelOptions {CancellationToken = _cancellationTokenSource.Token};
        }

        #region public methods

        #region ITripleStoreClient implementation

        public async Task<bool> CreateDataset(string name)
        {
            if (!await _triplestoreClient.CreateDataset(name))
            {
                return false;
            }

            return await _triplestoreClient.CreateGraph(name, MetadataGraphUri);
        }

        public async Task<bool> DeleteDataset(string name)
        {
            return await _triplestoreClient.DeleteDataset(name);
        }

        public async Task<IEnumerable<string>> ListDatasets()
        {
            return await _triplestoreClient.ListDatasets();
        }

        public async Task<bool> DeleteGraphs(string dataset, IEnumerable<Uri> graphUris)
        {
            var graphUriList = graphUris.ToList();
            if (graphUriList.Contains(MetadataGraphUri))
            {
                Warning("Cannot remove the metadata graph!");
                return false;
            }

            if (!await _triplestoreClient.DeleteGraphs(dataset, graphUriList))
            {
                return false;
            }

            var metadataTriples = (await GetMetadataTriples(dataset))?
                .Where(t => graphUriList.Contains(new Uri(t.Subject.ToString())));

            if (metadataTriples != null)
            {
                var triplesToRemove = metadataTriples.Select(triple => triple.ToString()).ToList();

                return await _triplestoreClient.UpdateGraphs(dataset,
                    new Dictionary<Uri, (IList<string>, IList<string>)>
                    {
                        [MetadataGraphUri] = (triplesToRemove, null)
                    });
            }

            return true;
        }

        /// <summary>
        /// Updates graphs with regards to active quality checks for each graph and whole dataset.
        /// </summary>
        public async Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)> triplesByGraphUri)
        {
            var triplesToAdd = triplesByGraphUri.Values.SelectMany(t => t.TriplesToAdd).ToList();
            if (!triplesToAdd.Any())
            {
                return await _triplestoreClient.UpdateGraphs(dataset, triplesByGraphUri);
            }

            //metadata should be added exclusively to force the use of quality check with given parameter to any data added in the future (per graph or per dataset)
            if (triplesByGraphUri.ContainsKey(MetadataGraphUri))
            {
                if (triplesByGraphUri.Keys.Any(k => k != MetadataGraphUri))
                {
                    Warning("Cannot directly modify metadata when other data is modified");
                    return false;
                }

                if (!await CanAddMetadataTriples(dataset, triplesToAdd))
                {
                    Verbose(TransactionAborted);
                    return false;
                }

                return await _triplestoreClient.UpdateGraphs(dataset, triplesByGraphUri);
            }

            if (!await CanAddGraphTriples(dataset, triplesByGraphUri))
            {
                Verbose(TransactionAborted);
                return false;
            }

            return await _triplestoreClient.UpdateGraphs(dataset, triplesByGraphUri);
        }

        public async Task<IEnumerable<IGraph>> ReadGraphs(string dataset, IEnumerable<Uri> graphUris)
        {
            return await _triplestoreClient.ReadGraphs(dataset, graphUris);
        }

        public async Task<IEnumerable<Uri>> ListGraphs(string dataset)
        {
            return await _triplestoreClient.ListGraphs(dataset);
        }

        public async Task<SparqlResultSet> RunSparqlQuery(string dataset, IEnumerable<Uri> graphs, string query)
        {
            return await _triplestoreClient.RunSparqlQuery(dataset, graphs, query);
        }

        public async Task<bool> CreateGraph(string dataset, Uri graphUri)
        {
            return await _triplestoreClient.CreateGraph(dataset, graphUri);
        }

        public void CancelOperation()
        {
            _triplestoreClient.CancelOperation();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _parallelOptions = new ParallelOptions { CancellationToken = _cancellationTokenSource.Token };
        }

        #endregion //ITriplestoreClient implementation

        #region ITriplestoreClientExtended implementation

        public async Task<bool> RevertLastTransaction(string storename)
        {
            if (_triplestoreClient is ITriplestoreClientExtended triplestoreClientExtended)
            {
                return await triplestoreClientExtended.RevertLastTransaction(storename);
            }

            return false;
        }

        public async Task<IEnumerable<(ulong id, DateTime commitDate)>> ListCommitPoints(string storename, int limit = 100)
        {
            if (_triplestoreClient is ITriplestoreClientExtended triplestoreClientExtended)
            {
                return await triplestoreClientExtended.ListCommitPoints(storename, limit);
            }

            return null;
        }

        public async Task<bool> RevertToCommitPoint(string storename, ulong commitId)
        {
            if (_triplestoreClient is ITriplestoreClientExtended triplestoreClientExtended)
            {
                return await triplestoreClientExtended.RevertToCommitPoint(storename, commitId);
            }

            return false;
        }

        public async Task<string> GetStatistics(string storeName)
        {
            if (_triplestoreClient is ITriplestoreClientExtended triplestoreClientExtended)
            {
                return await triplestoreClientExtended.GetStatistics(storeName);
            }

            return null;
        }

        #endregion //ITriplestoreClientExtended implementation

        #region IBrightstarClient implementation

        public async Task<bool> ConsolidateDataset(string storeName)
        {
            if (_triplestoreClient is IBrightstarClient brightstarClient)
            {
                return await brightstarClient.ConsolidateDataset(storeName);
            }

            return false;
        }

        #endregion //IBrightstarClient implementation

        #region ITriplestoreClientQualityWrapper implementation

        public async Task<IEnumerable<Triple>> GetMetadataTriples(string dataset)
        {
            var metadataTriples = await _triplestoreClient.ReadGraphs(dataset, MetadataGraphUri.AsEnumerable());

            var metadataGraph = metadataTriples.FirstOrDefault();
            return metadataGraph?.Triples;
        }

        #endregion //ITriplestoreClientQualityWrapper implementation

        #endregion //public methods 

        #region private methods

        private static Dictionary<IQualityCheck, List<string>> GetQualityChecksFromTriples(IEnumerable<string> triples)
        {
            var qualityChecks = new Dictionary<IQualityCheck, List<string>>();
            foreach (var triple in triples)
            {
                var qualityCheck = QualityCheckInstances.FirstOrDefault(qc => qc.GetPredicate() == triple.Predicate());
                if (qualityCheck != null)
                {
                    if (!qualityChecks.ContainsKey(qualityCheck))
                    {
                        qualityChecks[qualityCheck] = new List<string>();
                    }

                    qualityChecks[qualityCheck].Add(triple.Object());
                }
            }

            return qualityChecks;
        }

        private async Task<bool> CanAddMetadataTriples(string dataset, IReadOnlyCollection<string> triplesToAdd)
        {
            var graphList = await _triplestoreClient.ListGraphs(dataset);
            if (graphList == null)
            {
                Verbose("Transaction failed due to connection error");
                return false;
            }

            var graphs = (await _triplestoreClient.ReadGraphs(dataset, graphList))?
                .Where(g => g.BaseUri != MetadataGraphUri)
                .ToList();

            if (graphs == null)
            {
                Verbose("Transaction failed due to connection error");
                return false;
            }

            //Quality checks regarding whole dataset
            var datasetTriples = triplesToAdd.Where(t => t.Subject() == WholeDatasetSubjectUri.ToString())
                .ToList();

            var qualityChecksPassed = QualityChecksForWholeDatasetPassed(graphs, datasetTriples);
            if (!qualityChecksPassed)
            {
                Verbose(TransactionAborted);
                return false;
            }

            //Quality checks regarding graphs
            var graphTriples = triplesToAdd.Except(datasetTriples)
                .ToList();

            var graphUris = graphTriples.Select(t => t.Subject())
                .Distinct();

            qualityChecksPassed = QualityChecksForGraphsPassed(graphs, graphTriples, graphUris);

            if (!qualityChecksPassed)
            {
                Verbose(TransactionAborted);
                return false;
            }

            return true;
        }

        //TODO: QualityChecksForGraphsPassed and QualityChecksForWholeDatasetPassed can have some common code extracted
        private bool QualityChecksForGraphsPassed(IReadOnlyCollection<IGraph> graphs, IReadOnlyCollection<string> graphTriples, IEnumerable<string> graphUris)
        {
            var qualityChecksPassed = true;
            foreach (var graphUri in graphUris)
            {
                var qualityChecks = GetQualityChecksFromTriples(graphTriples.Where(t => t.Subject() == graphUri));
                Parallel.ForEach(qualityChecks, _parallelOptions, qualityCheck =>
                {
                    var qualityCheckReport = qualityCheck.Key.CheckGraphs(graphs.Where(g => g.BaseUri.ToString() == graphUri),
                        qualityCheck.Value);
                    if (!qualityCheckReport.QualityCheckPassed)
                    {
                        Verbose($"{qualityCheck.Key.GetType().Name} for graph {graphUri} failed");
                        qualityChecksPassed = false;
                    }
                });
            }

            return qualityChecksPassed;
        }

        private bool QualityChecksForWholeDatasetPassed(IEnumerable<IGraph> graphs, IEnumerable<string> datasetTriples)
        {
            var datasetQualityChecks = GetQualityChecksFromTriples(datasetTriples);
            var qualityChecksPassed = true;

            var graphList = graphs.ToList();
            if (graphList.Any())
            {
                Parallel.ForEach(datasetQualityChecks, _parallelOptions, qualityCheck =>
                {
                    var qualityCheckReport = qualityCheck.Key.CheckGraphs(graphList, qualityCheck.Value);
                    if (!qualityCheckReport.QualityCheckPassed)
                    {
                        Verbose($"{qualityCheck.Key.GetType().Name} for whole dataset failed");
                        qualityChecksPassed = false;
                    }
                });
            }

            return qualityChecksPassed;
        }

        private async Task<bool> CanAddGraphTriples(string dataset, Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)> triplesByGraphUri)
        {
            var activeQualityChecks = (await GetMetadataTriples(dataset))?
                .ToList();

            if (activeQualityChecks == null)
            {
                Verbose(TransactionAborted);
                return false;
            }

            if (!activeQualityChecks.Any())
            {
                return true;
            }

            var graphQualityChecksPassed = true;
            var _lock = new object();
            foreach (var graphUri in triplesByGraphUri.Keys)
            {
                var graphQualityCheckTriples = activeQualityChecks.Where(t => t.Subject.ToString() == WholeDatasetSubjectUri.ToString() || t.Subject.ToString() == graphUri.ToString())
                    .Select(t => t.ToString());

                var graphQualityChecks = GetQualityChecksFromTriples(graphQualityCheckTriples);
                Parallel.ForEach(graphQualityChecks, _parallelOptions, qualityCheck =>
                {
                    var qualityCheckReport = qualityCheck.Key.CheckData(triplesByGraphUri[graphUri].TriplesToAdd, qualityCheck.Value);
                    if (!qualityCheckReport.QualityCheckPassed)
                    {
                        Verbose($"{qualityCheck.Key.GetType().Name} for graph {graphUri} failed, transaction aborted");
                        lock (_lock)
                        {
                            graphQualityChecksPassed = false;
                        }
                    }
                });
            }

            if (!graphQualityChecksPassed)
            {
                Verbose(TransactionAborted);
                return false;
            }

            return true;
        }

        #endregion
    }
}
