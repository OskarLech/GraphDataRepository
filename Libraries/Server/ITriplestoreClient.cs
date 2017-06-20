using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace Libraries.Server
{
    /// <summary>
    /// Generic interface for basic CRUD operations any triplestore should provide
    /// </summary>
    public interface ITriplestoreClient
    {
        Task<bool> CreateDataset(string name);
        Task<bool> DeleteDataset(string name);
        Task<IEnumerable<string>> ListDatasets();
        Task<bool> DeleteGraphs(string dataset, IEnumerable<Uri> graphUris);

        Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)> triplesByGraphUri);
        Task<IEnumerable<IGraph>> ReadGraphs(string dataset, IEnumerable<Uri> graphUris);
        Task<IEnumerable<Uri>> ListGraphs(string dataset);
        Task<SparqlResultSet> RunSparqlQuery(string dataset, IEnumerable<Uri> graphs, string query);
    }
}
