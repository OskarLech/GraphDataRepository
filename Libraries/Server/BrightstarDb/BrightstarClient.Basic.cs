using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightstarDB.Client;
using Common;
using static Serilog.Log;

namespace Libraries.Server.BrightstarDb
{
    /// <summary>
    /// Handles communication with BrightstarDB endpoint
    /// </summary>
    public partial class BrightstarClient : TriplestoreClient
    {
        private readonly IBrightstarService _brightstarClient;

        public BrightstarClient(string endpoint) : base(endpoint)
        {
            _brightstarClient = BrightstarService.GetClient($"Type=rest;endpoint={endpoint};");
        }

        public override async Task<bool> CreateDataset(string name)
        {
            return await ClientCall(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    Debug("Dataset name cannot be empty");
                    return false;
                }

                if (!_brightstarClient.DoesStoreExist(name))
                {
                    _brightstarClient.CreateStore(name);
                    return true;
                }

                Debug($"Dataset {name} already exists");
                return false;
            }, CancellationTokenSource.Token));
        }

        public override async Task<bool> DeleteDataset(string name)
        {
            return await ClientCall(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    Debug("Dataset name cannot be empty");
                    return false;
                }

                if (_brightstarClient.DoesStoreExist(name))
                {
                    _brightstarClient.DeleteStore(name);
                    return true;
                }

                Debug($"Tried to delete non-existing dataset {name}");
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

        public override async Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)> triplesByGraphUri)
        {
            return await ClientCall(Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(dataset) || triplesByGraphUri.Any(t => string.IsNullOrWhiteSpace(t.Key.ToString())))
                {
                    Debug("Dataset and graph URI cannot be empty");
                    return false;
                }

                if (triplesByGraphUri.Values.Any(v => v.TriplesToAdd == null && v.TriplesToRemove == null))
                {
                    Warning("Cannot run update graph operation with no arguments");
                    return false;
                }

                var deletePatterns = new StringBuilder();
                var insertData = new StringBuilder();
                foreach (var triples in triplesByGraphUri)
                {
                    if (triples.Value.TriplesToRemove != null)
                    {
                        foreach (var triple in triples.Value.TriplesToRemove)
                        {
                            deletePatterns.AppendLine($"{ConvertToBrightstarCompatibleTriple(triple)}<{triples.Key}> .");
                        }
                    }

                    if (triples.Value.TriplesToAdd != null)
                    {
                        foreach (var triple in triples.Value.TriplesToAdd)
                        {
                            insertData.AppendLine($"{ConvertToBrightstarCompatibleTriple(triple)}<{triples.Key}> .");
                        }
                    }
                }

                var transactionData = new UpdateTransactionData
                {
                    DeletePatterns = deletePatterns.ToString(),
                    InsertData = insertData.ToString()
                };

                var jobInfo = _brightstarClient.ExecuteTransaction(dataset, transactionData);
                if (!jobInfo.JobCompletedOk)
                {
                    Error($"BrightstarDB transactional update failed: {jobInfo.ExceptionInfo}");
                    return false;
                }

                return true;
            }, CancellationTokenSource.Token));
        }

        public override async Task<bool> CreateCommitPoint(string dataset)
        {
            return await ClientCall(Task.Run(() =>
            {
                var jobInfo = _brightstarClient.ExecuteTransaction(dataset, new UpdateTransactionData());
                if (!jobInfo.JobCompletedOk)
                {
                    Error($"BrightstarDB transactional update failed: {jobInfo.ExceptionInfo}");
                    return false;
                }

                return true;
            }, CancellationTokenSource.Token));
        }

        private static string ConvertToBrightstarCompatibleTriple(string triple)
        {
            var tripleNodes = new[] {triple.Subject(), triple.Predicate(), triple.Object()};

            var result = "";
            for (var i = 0; i < 3; i++)
            {
                var node = tripleNodes[i];
                if (string.IsNullOrWhiteSpace(node))
                {
                    continue;
                }

                if (i == 2)
                {
                    if (!Uri.TryCreate(node, UriKind.Absolute, out var dummy))
                    {
                        result = result + node.Insert(node.Length, "\" ").Insert(0, "\"");
                        continue;
                    }
                }

                result = result + node.Insert(node.Length, "> ").Insert(0, "<");
            }

            return result;
        }
    }
}
