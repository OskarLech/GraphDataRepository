using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace GraphDataRepository.Server
{
    /// <summary>
    /// Generic interface for basic CRUD operations any triplestore should provide
    /// </summary>
    public interface ITriplestoreClient : IDisposable
    {
        Task<bool> CreateDataset(string name);
        Task<bool> DeleteDataset(string name);
        Task<IEnumerable<string>> ListDatasets();
        Task<bool> DeleteGraph(string dataset, string graphUri);
        Task<bool> UpdateGraph(string dataset, string graphUri, IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd);
        Task<IGraph> ReadGraph(string dataset, string graphUri);
        Task<IEnumerable<Uri>> ListGraphs(string dataset);
        Task<SparqlResultSet> RunSparqlQuery(string dataset, IEnumerable<Uri> graphs, string query);
    }
}
