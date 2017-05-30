using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BrightstarDB.Client;
using static Serilog.Log;

namespace GraphDataRepository.Server.BrightstarDb
{
    /// <summary>
    /// Handles communication with BrightstarDB endpoint
    /// </summary>
    internal class BrightstarClient : TriplestoreClient, IBrightstarClient
    {
        //TODO add some authentication mechanisms
        private readonly IBrightstarService _brightstarClient;

        #region public methods
        public BrightstarClient(string endpoint) : base(endpoint)
        {
            _brightstarClient = BrightstarService.GetClient($"Type=rest;endpoint={endpoint};");
        }

        public override async Task<bool> CreateDataset(string name)
        {
            return await ClientCall(Task.Run(() =>
            {
                if (string.IsNullOrEmpty(name))
                {
                    Logger.Debug("Dataset name cannot be empty");
                    return false;
                }

                if (!_brightstarClient.DoesStoreExist(name))
                {
                    _brightstarClient.CreateStore(name);
                    return true;
                }

                Logger.Debug($"Dataset {name} already exists");
                return false;
            }, CancellationTokenSource.Token));
        }

        public override async Task<bool> DeleteDataset(string name)
        {
            return await ClientCall(Task.Run(() =>
            {
                if (string.IsNullOrEmpty(name))
                {
                    Logger.Debug("Dataset name cannot be empty");
                    return false;
                }

                if (_brightstarClient.DoesStoreExist(name))
                {
                    _brightstarClient.DeleteStore(name);
                    return true;
                }

                Logger.Debug($"Tried to delete non-existing dataset {name}");
                return false;
            }, CancellationTokenSource.Token));
        }

        public override async Task<IEnumerable<string>> ListDatasets()
        {
            return await ClientCall(Task.Run(() =>
            {
                var datasetList = _brightstarClient.ListStores() ?? new List<string>();
                return datasetList;
            }, CancellationTokenSource.Token));
        }

        public override async Task<bool> UpdateGraph(string dataset, Uri graphUri, IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)
        {
            return await ClientCall(Task.Run(() =>
            {
                if (string.IsNullOrEmpty(dataset) || string.IsNullOrEmpty(graphUri.ToString()))
                {
                    Logger.Debug("Dataset and graph URI cannot be empty");
                    return false;
                }

                var deletePatterns = new StringBuilder();
                foreach (var triple in triplesToRemove)
                {
                    deletePatterns.AppendLine($"{triple} <{graphUri}> .");
                }

                var insertData = new StringBuilder();
                foreach (var triple in triplesToAdd)
                {
                    insertData.AppendLine($"{triple} <{graphUri}> .");
                }

                var transactionData = new UpdateTransactionData
                {
                    DeletePatterns = deletePatterns.ToString(),
                    InsertData = insertData.ToString()
                };

                var jobInfo = _brightstarClient.ExecuteTransaction(dataset, transactionData);
                if (!jobInfo.JobCompletedOk)
                {
                    Logger.Error($"BrightstarDB transactional update failed: {jobInfo.ExceptionInfo}");
                    return false;
                }

                return true;
            }, CancellationTokenSource.Token));
        }
        #endregion
    }
}
