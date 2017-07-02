using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace Libraries.Server
{
    public class TriplestoreClientQualityWrapper : ITriplestoreClientQualityWrapper
    {
        private readonly Uri _metadataGraphUri = new Uri("resource://metadata"); 
        private readonly ITriplestoreClient _triplestoreClient;
        private readonly IEnumerable<SupportedTriplestores.SupportedOperations> _supportedOperations;

        public TriplestoreClientQualityWrapper(ITriplestoreClient triplestoreClient)
        {
            _triplestoreClient = triplestoreClient;
            _supportedOperations = SupportedTriplestores.GetSupportedOperations(triplestoreClient.GetType());
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
                    [_metadataGraphUri] = (null, null)
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

        public Task<bool> DeleteGraphs(string dataset, IEnumerable<Uri> graphUris)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)> triplesByGraphUri)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IGraph>> ReadGraphs(string dataset, IEnumerable<Uri> graphUris)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Uri>> ListGraphs(string dataset)
        {
            throw new NotImplementedException();
        }

        public Task<SparqlResultSet> RunSparqlQuery(string dataset, IEnumerable<Uri> graphs, string query)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RevertLastTransaction(string storename)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<(ulong id, DateTime commitDate)>> ListCommitPoints(string storename, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RevertToCommitPoint(string storename, ulong commitId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetStatistics(string storeName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConsolidateStore(string storeName)
        {
            throw new NotImplementedException();
        }
    }
}
