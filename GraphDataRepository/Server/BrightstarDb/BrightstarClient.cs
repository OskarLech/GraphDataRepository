using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BrightstarDB.Client;
using log4net;

namespace GraphDataRepository.Server.BrightstarDb
{
    /// <summary>
    /// Handles communication with BrightstarDB endpoint
    /// </summary>
    internal class BrightstarClient : TriplestoreClient, IBrightstarClient
    {
        private IBrightstarService _brightstarClient;

        #region public methods
        public BrightstarClient(ILog log, string endpoint) : base(log, endpoint)
        {
            _brightstarClient = BrightstarService.GetClient($"Type=rest;endpoint={endpoint};");
        }

        public override async Task<bool> CreateDataset(string name)
        {
            return await ClientCall(new Task<bool>(() =>
            {
                if (!_brightstarClient.DoesStoreExist(name))
                {
                    _brightstarClient.CreateStore(name);
                    return true;
                }

                Log.Debug($"Dataset {name} already exists");
                return false;
            }, CancellationTokenSource.Token));
        }

        public override async Task<bool> DeleteDataset(string name)
        {
            return await ClientCall(new Task<bool>(() =>
            {
                if (_brightstarClient.DoesStoreExist(name))
                {
                    _brightstarClient.DeleteStore(name);
                    return true;
                }

                Log.Debug($"Tried to delete non-existing dataset {name}");
                return false;
            }, CancellationTokenSource.Token));
        }

        public override async Task<IEnumerable<string>> ListDatasets()
        {
            return await ClientCall(new Task<IEnumerable<string>>(() =>
            {
                var datasetList = _brightstarClient.ListStores() ?? new List<string>();
                return datasetList;
            }, CancellationTokenSource.Token));
        }

        public override async Task<bool> UpdateGraph(string dataset, string graphUri, IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)
        {
            return await ClientCall(new Task<bool>(() =>
            {
                var deletePatterns = new StringBuilder();
                foreach (var triple in triplesToRemove)
                {
                    deletePatterns.AppendLine($"{triple} {graphUri} .");
                }

                var insertData = new StringBuilder();
                foreach (var triple in triplesToAdd)
                {
                    insertData.AppendLine($"{triple} {graphUri} .");
                }

                var transactionData = new UpdateTransactionData
                {
                    DeletePatterns = deletePatterns.ToString(),
                    InsertData = insertData.ToString()
                };

                var jobInfo = _brightstarClient.ExecuteTransaction(dataset, transactionData);
                if (!jobInfo.JobCompletedOk)
                {
                    Log.Error($"BrightstarDB transactional update failed: {jobInfo.ExceptionInfo}");
                    return false;
                }

                return true;
            }));
        }

        public override async Task<IEnumerable<string>> ListGraphs(string dataset)
        {
            return await ClientCall(new Task<IEnumerable<string>>(() => _brightstarClient.ListNamedGraphs(dataset) ?? new List<string>()));
        }
        #endregion

        protected override void OnDispose(bool disposing)
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
            HttpClient.Dispose();
            _brightstarClient = null;

            Log.Debug("BrightstarClient disposed");
        }
    }
}
