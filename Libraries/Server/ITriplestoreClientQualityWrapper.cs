using System.Collections.Generic;
using System.Threading.Tasks;
using Libraries.Server.BrightstarDb;
using VDS.RDF;

namespace Libraries.Server
{
    /// <summary>
    /// Interface for triplestore wrapper that handles metadata and quality checks aside from server communication.
    /// Should implement iterfaces of all triplestores.
    /// </summary>
    public interface ITriplestoreClientQualityWrapper : IBrightstarClient
    {
        Task<IEnumerable<Triple>> GetMetadataTriples(string dataset);
    }
}
