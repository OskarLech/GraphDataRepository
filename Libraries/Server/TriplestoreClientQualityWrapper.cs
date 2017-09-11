using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ITriplestoreClientBasic _triplestoreClient;

        public TriplestoreClientQualityWrapper(ITriplestoreClientBasic triplestoreClient)
        {
            _triplestoreClient = triplestoreClient;
        }

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
            var (relatedGraphs, metadataTriples) = await GetRelatedGraphsAndMetadataTriples(dataset, triplesByGraphUri);

            var qualityChecksPassed = true;
            foreach (var graphUri in relatedGraphs)
            {
                var graphQualityChecks = GetQualityChecksFromTriples(metadataTriples.Where(t =>
                    !string.IsNullOrWhiteSpace(t) &&
                    (t.Subject() == graphUri.ToString() ||
                    t.Subject() == WholeDatasetSubjectUri.ToString())));

                var triplesToAddInGraph = triplesByGraphUri.Where(t => t.Key == graphUri).
                    SelectMany(t => t.Value.TriplesToAdd)
                    .ToList();

                var triplesToRemoveInGraph = triplesByGraphUri.Where(t => t.Key == graphUri)
                    .SelectMany(t => t.Value.TriplesToRemove)
                    .ToList();

                if (!await AreQualityCheckCriteriaMet(dataset, graphUri, graphQualityChecks, (triplesToAddInGraph, triplesToRemoveInGraph)))
                {
                    qualityChecksPassed = false;
                }
            }

            if (qualityChecksPassed)
            {
                return await _triplestoreClient.UpdateGraphs(dataset, triplesByGraphUri);
            }

            return false;
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
            //TODO validate queries or at least run checks before / after
            return await _triplestoreClient.RunSparqlQuery(dataset, graphs, query);
        }

        public async Task<bool> CreateGraph(string dataset, Uri graphUri)
        {
            return await _triplestoreClient.CreateGraph(dataset, graphUri);
        }

        public void CancelOperation()
        {
            _triplestoreClient.CancelOperation();
        }

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

        public async Task<bool> ConsolidateDataset(string storeName)
        {
            if (_triplestoreClient is IBrightstarClient brightstarClient)
            {
                return await brightstarClient.ConsolidateDataset(storeName);
            }

            return false;
        }

        public async Task<IEnumerable<Triple>> GetMetadataTriples(string dataset)
        {
            var metadataGraph = (await _triplestoreClient.ReadGraphs(dataset, MetadataGraphUri.AsEnumerable()))?
                .FirstOrDefault();

            return metadataGraph?.Triples;
        }

        private static Dictionary<IQualityCheck, List<string>> GetQualityChecksFromTriples(IEnumerable<string> triples)
        {
            var qualityChecks = new Dictionary<IQualityCheck, List<string>>();
            foreach (var triple in triples)
            {
                var qualityCheck = QualityCheckInstances.FirstOrDefault(qc => string.Equals(qc.GetPredicate(), triple.Predicate(), StringComparison.CurrentCultureIgnoreCase));
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

        private async Task<(List<Uri> relatedGraphs, List<string> metadataTriples)> GetRelatedGraphsAndMetadataTriples(string dataset,
            Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)> triplesByGraphUri)
        {
            var relatedGraphs = new List<Uri>();
            var metadataTriples = (await GetMetadataTriples(dataset))?.Select(t => t.ToString())
                .ToList();

            if (metadataTriples == null) return default((List<Uri>, List<string>));

            if (triplesByGraphUri.ContainsKey(MetadataGraphUri))
            {
                if (triplesByGraphUri.Keys.Any(k => k != MetadataGraphUri))
                {
                    Warning("Cannot directly modify metadata when other data is modified");
                    return default((List<Uri>, List<string>));
                }

                var datasetTriplesToAdd = triplesByGraphUri[MetadataGraphUri].TriplesToAdd;
                var datasetTriplesToRemove = triplesByGraphUri[MetadataGraphUri].TriplesToRemove;

                //if modyfying quality checks for whole dataset each graph has to be checked
                if (datasetTriplesToAdd.Any(t => t.Subject() == WholeDatasetSubjectUri.ToString()) ||
                    datasetTriplesToRemove.Any(t => t.Subject() == WholeDatasetSubjectUri.ToString()))
                {
                    relatedGraphs = (await ListGraphs(dataset))?.ToList();
                    if (relatedGraphs == null) return default((List<Uri>, List<string>));
                }
                else //otherwise just add each graph separately
                {
                    relatedGraphs.AddRange(datasetTriplesToAdd.Union(datasetTriplesToRemove)
                        .Select(t => t.Subject())
                        .Select(graphUri => new Uri(graphUri)));
                }

                //metadata triples after changes
                metadataTriples = metadataTriples
                    .Except(datasetTriplesToRemove)
                    .Union(datasetTriplesToAdd)
                    .ToList();
            }
            else //if quality checks are not edited just add every modified graph
            {
                relatedGraphs.AddRange(triplesByGraphUri.Keys);
            }

            return (relatedGraphs.Distinct().ToList(), metadataTriples);
        }

        private async Task<bool> AreQualityCheckCriteriaMet(string dataset, Uri graphUri, Dictionary<IQualityCheck, List<string>> qualityChecks,
            (IList<string> TriplesToRemove, IList<string> TriplesToAdd) triplesToModify = default((IList<string>, IList<string>)))
        {
            var graphTriples = (await ReadGraphs(dataset, graphUri.AsEnumerable()))?.FirstOrDefault()?
                .Triples?.Select(t => t.ToString())
                .ToList();

            if (graphTriples == null) return false;

            if (triplesToModify.TriplesToRemove != null)
            {
                graphTriples = graphTriples.Except(triplesToModify.TriplesToRemove)
                    .ToList();
            }

            if (triplesToModify.TriplesToAdd != null)
            {
                graphTriples.AddRange(triplesToModify.TriplesToAdd);
            }

            return qualityChecks.Select(qualityCheck => qualityCheck.Key.CheckData(graphTriples, qualityCheck.Value))
                .All(qualityCheckReport => qualityCheckReport.QualityCheckPassed);
        }
    }
}
