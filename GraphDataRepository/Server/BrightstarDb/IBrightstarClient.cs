using System.Threading.Tasks;

namespace GraphDataRepository.Server.BrightstarDb
{
    /// <summary>
    /// Functionalities specific to BrightstarDB
    /// </summary>
    internal interface IBrightstarClient
    {
        Task<bool> ConsolidateStore(string storeName);
    }
}
