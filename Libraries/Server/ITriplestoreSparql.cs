using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDS.RDF;
using VDS.RDF.Query;

namespace Libraries.Server
{
    /// <summary>
    /// Generic interface for every endpoint with SPARQL support
    /// </summary>
    public interface ITriplestoreSparql
    {
        Task<IEnumerable<IGraph>> ReadGraphs(string dataset, IEnumerable<Uri> graphUris);
        Task<IEnumerable<Uri>> ListGraphs(string dataset);
        Task<SparqlResultSet> RunSparqlQuery(string dataset, IEnumerable<Uri> graphs, string query);
    }
}
