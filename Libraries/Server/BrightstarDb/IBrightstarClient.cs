using System.Threading.Tasks;

namespace Libraries.Server.BrightstarDb
{
    /// <summary>
    /// Functionalities specific to BrightstarDB
    /// </summary>
    public interface IBrightstarClient : ITriplestoreClientExtended
    {
        Task<bool> ConsolidateStore(string storeName);
    }
}
