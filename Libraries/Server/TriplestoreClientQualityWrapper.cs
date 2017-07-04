using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libraries.QualityChecks;
using Libraries.Server.BrightstarDb;
using VDS.RDF;
using VDS.RDF.Query;
using static Serilog.Log;

namespace Libraries.Server
{
    /// <summary>
    /// Wraps ITriplestoreClient to enforce quality checks fulfillment
    /// </summary>
    public class TriplestoreClientQualityWrapper : ITriplestoreClientQualityWrapper
    {
        public const string WholeDatasetSubject = "WholeDataset";
        private static readonly Uri MetadataGraphUri = new Uri("resource://metadata");
        private readonly ITriplestoreClient _triplestoreClient;
        private readonly IEnumerable<IQualityCheck> _qualityChecks;

        public TriplestoreClientQualityWrapper(ITriplestoreClient triplestoreClient)
        {
            _triplestoreClient = triplestoreClient;
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
                    [MetadataGraphUri] = (null, null)
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
            if (!await _triplestoreClient.DeleteGraphs(dataset, graphUriList))
            {
                return false;
            }

            var metadataTriples = (await GetMetadataTriples(dataset, graphUriList))?
                .Where(t => t.Subject.ToString() != WholeDatasetSubject);
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

        public async Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IEnumerable<string> TriplesToRemove, IEnumerable<string> TriplesToAdd)> triplesByGraphUri)
        {
            //metadata should be added exclusively to force the use of quality check with given parameter to any data added in the future (per graph or per dataset)
            if (triplesByGraphUri.ContainsKey(MetadataGraphUri))
            {
                if (triplesByGraphUri.Keys.Any(k => k != MetadataGraphUri))
                {
                    Warning("Cannot directly modify metadata when other data is modified");
                    return false;
                }

                return await _triplestoreClient.UpdateGraphs(dataset, triplesByGraphUri);
            }

            var metadataTriples = (await GetMetadataTriples(dataset, triplesByGraphUri.Keys.ToList()))?
                .ToList();

            if (metadataTriples != null && metadataTriples.Any())
            {
                var fulfilledQualityChecks = GetQualityChecksWithParameters(metadataTriples);
                
                var triplesToRemove = triplesByGraphUri.SelectMany(t => t.Value.TriplesToRemove).ToList();
                var metadataTriplesToRemove = GetMetadataTriplesToRemove(fulfilledQualityChecks, triplesToRemove).ToList();
                triplesByGraphUri[MetadataGraphUri] = (metadataTriplesToRemove, new List<string>());
                
                //Some of the quality checks might not be fulfilled anymore after triple deletion
                fulfilledQualityChecks = GetQualityChecksWithParameters(metadataTriples.Where(t => !metadataTriplesToRemove.Any(mttr => Equals(mttr, t.ToString())))); //TODO check if this works

                var triplesToAdd = triplesByGraphUri.SelectMany(t => t.Value.TriplesToAdd)
                    .Distinct(); //there can be same triples in more than one graph

                if (!TriplesFullfilQualityChecks(triplesToAdd, fulfilledQualityChecks))
                {
                    return false;
                }

                return await _triplestoreClient.UpdateGraphs(dataset, triplesByGraphUri);
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

        #endregion

        #region ITriplestoreExtended implementation

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

        #endregion

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

        #endregion //public methods 

        #region private methods

        private static IEnumerable<IQualityCheck> GetQualityChecks()
        {
            return SupportedQualityChecks.QualityChecksList.Select(qualityCheck => (IQualityCheck) Activator.CreateInstance(qualityCheck.qualityCheckClass)).ToList();
        }

        private async Task<IEnumerable<Triple>> GetMetadataTriples(string dataset, IEnumerable<Uri> graphUriList)
        {
            var metadataGraph = (await _triplestoreClient.ReadGraphs(dataset, MetadataGraphUri.AsEnumerable())).FirstOrDefault();
            var metadataTriples = metadataGraph?.Triples.Where(t => t.Subject.ToString() == WholeDatasetSubject || graphUriList.Contains(new Uri(t.Subject.ToString())));
            return metadataTriples;
        }

        private Dictionary<IQualityCheck, IEnumerable<string>> GetQualityChecksWithParameters(IEnumerable<Triple> metadataTriples)
        {
            var qualityCheckPredicates = _qualityChecks.Select(p => p.GetPredicate());
            var qualityCheckTriples = metadataTriples.Where(t => qualityCheckPredicates
                 .Any(p => p == t.Predicate.ToString()))
                .ToList();

            var fulfilledQualityChecks = _qualityChecks.Where(q => qualityCheckTriples.Any(t => t.Predicate.ToString() == q.GetPredicate()))
                .ToList();

            var qualityChecksWithParameters = new Dictionary<IQualityCheck, IEnumerable<string>>();
            foreach (var qualityCheck in fulfilledQualityChecks)
            {
                qualityChecksWithParameters[qualityCheck] = qualityCheckTriples
                    .Where(t => t.Predicate.ToString() == qualityCheck.GetPredicate())
                    .Select(t => t.Object.ToString());
            }

            return qualityChecksWithParameters;
        }

        private IEnumerable<string> GetMetadataTriplesToRemove(Dictionary<IQualityCheck, IEnumerable<string>> fulfilledQualityChecks, IEnumerable<string> selectMany)
        {
            throw new NotImplementedException();
        }

        private bool TriplesFullfilQualityChecks(IEnumerable<string> triples, Dictionary<IQualityCheck, IEnumerable<string>> qualityChecksWithParameters)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
