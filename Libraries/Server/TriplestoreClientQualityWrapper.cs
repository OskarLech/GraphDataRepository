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
        private readonly IEnumerable<IQualityCheck> _qualityChecks;

        private CancellationTokenSource _cancellationTokenSource;
        private ParallelOptions _parallelOptions;

        public TriplestoreClientQualityWrapper(ITriplestoreClient triplestoreClient)
        {
            _triplestoreClient = triplestoreClient;
            _cancellationTokenSource = new CancellationTokenSource();
            _parallelOptions = new ParallelOptions {CancellationToken = _cancellationTokenSource.Token};
            _qualityChecks = GetQualityChecks();
        }

        #region public methods

        #region ITripleStoreClient implementation
         
        public async Task<bool> CreateDataset(string name)
        {
            if (!await _triplestoreClient.CreateDataset(name))
            {
                return false;
            }

            //create metadata graph
            var triplesByGraphUri =
                new Dictionary<Uri, (IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)>
                {
                    [MetadataGraphUri] = (new List<string>(), new List<string> { "<http://www.brightstardb.com/companies/brightstardb> <http://www.w3.org/2000/01/rdf-schema#label> \"BrightstarDB\"" })
                };

            return await _triplestoreClient.UpdateGraphs(name, triplesByGraphUri);
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
                    new Dictionary<Uri, (IEnumerable<string>, IEnumerable<string>)>
                    {
                        [MetadataGraphUri] = (triplesToRemove, null)
                    });
            }

            return true;
        }

        /// <summary>
        /// Updates graphs with regards to active quality checks for each graph and whole dataset.
        /// </summary>
        public async Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IEnumerable<string> TriplesToRemove, IEnumerable<string> TriplesToAdd)> triplesByGraphUri)
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

        public async Task<bool> ConsolidateStore(string storeName)
        {
            if (_triplestoreClient is IBrightstarClient brightstarClient)
            {
                return await brightstarClient.ConsolidateStore(storeName);
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

        private static IEnumerable<IQualityCheck> GetQualityChecks()
        {
            return SupportedQualityChecks.Select(qualityCheck => (IQualityCheck) Activator.CreateInstance(qualityCheck.qualityCheckClass)).ToList();
        }

        private async Task<bool> CanAddMetadataTriples(string dataset, IReadOnlyCollection<string> triplesToAdd)
        {
            var graphList = await _triplestoreClient.ListGraphs(dataset);
            if (graphList == null)
            {
                Verbose("Transaction failed due to connection error");
                return false;
            }

            var graphs = await _triplestoreClient.ReadGraphs(dataset, graphList);
            if (graphs == null)
            {
                Verbose("Transaction failed due to connection error");
                return false;
            }

            //Quality checks regarding whole dataset
            var datasetTriples = triplesToAdd.Where(t => t.Subject() == WholeDatasetSubject)
                .ToList();

            var datasetQualityChecks = GetQualityChecksFromTriples(datasetTriples);
            var qualityChecksPassed = true;
            Parallel.ForEach(datasetQualityChecks, _parallelOptions, qualityCheck =>
            {
                var qualityCheckReport = qualityCheck.Key.CheckGraphs(graphs, qualityCheck.Value);
                if (!qualityCheckReport.QualityCheckPassed)
                {
                    Verbose($"{qualityCheck.Key.GetType().Name} for whole dataset failed");
                    qualityChecksPassed = false;
                }
            });

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

            if (!qualityChecksPassed)
            {
                Verbose(TransactionAborted);
                return false;
            }

            return true;
        }

        private async Task<bool> CanAddGraphTriples(string dataset, Dictionary<Uri, ValueTuple<IEnumerable<string>, IEnumerable<string>>> triplesByGraphUri)
        {
            var activeQualityChecks = (await GetMetadataTriples(dataset))?
                .ToList();

            if (activeQualityChecks == null)
            {
                Verbose(TransactionAborted);
                return false;
            }

            var graphQualityChecksPassed = false;
            foreach (var graphUri in triplesByGraphUri.Keys)
            {
                var graphQualityCheckTriples = activeQualityChecks.Where(t => t.Subject.ToString() == WholeDatasetSubject || t.Subject.ToString() == graphUri.ToString())
                    .Select(t => t.ToString());

                var graphQualityChecks = GetQualityChecksFromTriples(graphQualityCheckTriples);
                Parallel.ForEach(graphQualityChecks, _parallelOptions, qualityCheck =>
                {
                    var qualityCheckReport = qualityCheck.Key.CheckData(triplesByGraphUri[graphUri].Item2,
                        qualityCheck.Value);
                    if (!qualityCheckReport.QualityCheckPassed)
                    {
                        Verbose($"{qualityCheck.Key.GetType().Name} for graph {graphUri} failed, transaction aborted");
                        graphQualityChecksPassed = false;
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

        private Dictionary<IQualityCheck, List<string>> GetQualityChecksFromTriples(IEnumerable<string> triples)
        {
            var qualityChecks = new Dictionary<IQualityCheck, List<string>>();
            foreach (var triple in triples)
            {
                var qualityCheck = _qualityChecks.First(qc => qc.GetPredicate() == triple.Predicate());
                qualityChecks[qualityCheck].Add(triple.Object());
            }

            return qualityChecks;
        }

        #endregion
    }
}
