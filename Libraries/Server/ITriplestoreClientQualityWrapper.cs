using Libraries.Server.BrightstarDb;

namespace Libraries.Server
{
    /// <summary>
    /// Interface for triplestore wrapper that handles metadata and quality checks aside from server communication.
    /// Should implement iterfaces of all triplestores.
    /// </summary>
    public interface ITriplestoreClientQualityWrapper : IBrightstarClient
    {
    }
}
