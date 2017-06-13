using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GraphDataRepository.Server
{
    /// <summary>
    /// Extends ITriplestoreClient by additional functionalities
    /// </summary>
    internal interface ITriplestoreClientExtended : ITriplestoreClient
    {
        Task<bool> RevertLastTransaction(string storename);
        Task<IEnumerable<(ulong id, DateTime commitDate)>> ListCommitPoints(string storename, int limit = 100);
        Task<bool> RevertToCommitPoint(string storename, ulong commitId);
        Task<string> GetStatistics(string storeName);
    }
}
