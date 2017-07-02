using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libraries.Server.BrightstarDb;
using VDS.RDF;
using VDS.RDF.Query;

namespace Libraries.Server
{
    public class TriplestoreClientQualityWrapper : ITriplestoreClientQualityWrapper
    {
        private static readonly Uri MetadataGraphUri = new Uri("resource://metadata"); 
        private readonly ITriplestoreClient _triplestoreClient;

        public TriplestoreClientQualityWrapper(ITriplestoreClient triplestoreClient)
        {
            _triplestoreClient = triplestoreClient;
        }

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
            var graphUriList = graphUris as IList<Uri> ?? graphUris.ToList(); //multple enumeration
            if (!await _triplestoreClient.DeleteGraphs(dataset, graphUriList))
            {
                return false;
            }

            var metadataTriples = await GetMetadataTriples(dataset, graphUriList);
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

        private async Task<IEnumerable<Triple>> GetMetadataTriples(string dataset, ICollection<Uri> graphUriList)
        {
            //delete from metadata as well
            var metadataGraph = (await _triplestoreClient.ReadGraphs(dataset, MetadataGraphUri.AsEnumerable())).FirstOrDefault();
            var metadataTriples = metadataGraph?.Triples.Where(t => graphUriList.Contains(new Uri(t.Subject.ToString())));
            return metadataTriples;
        }

        public async Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)> triplesByGraphUri)
        {
            var metadataTriples = await GetMetadataTriples(dataset, triplesByGraphUri.Keys.ToList());
            if (metadataTriples != null)
            {
                //TODO
            }

            throw new NotImplementedException();
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

        public async Task<bool> ConsolidateStore(string storeName)
        {
            if (_triplestoreClient is IBrightstarClient brightstarClient)
            {
                return await brightstarClient.ConsolidateStore(storeName);
            }

            return false;
        }
    }
}
