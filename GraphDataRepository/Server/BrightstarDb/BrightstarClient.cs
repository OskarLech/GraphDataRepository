using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrightstarDB.Client;
using Common;
using log4net;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace GraphDataRepository.Server.BrightstarDb
{
    /// <summary>
    /// Handles communication with BrightstarDB endpoint
    /// </summary>
    internal class BrightstarClient : Disposable, IBrightstarClient
    {
        private IBrightstarService _brightstarClient;
        private readonly string _endpointUri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        #region public methods
        public BrightstarClient(ILog log, string endpoint) : base(log)
        {
            _endpointUri = endpoint;
            _brightstarClient = BrightstarService.GetClient($"Type=rest;endpoint={endpoint};");
        }

        public async Task<bool> CreateDataset(string name)
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
            }, _cancellationTokenSource.Token));
        }

        public async Task<bool> DeleteDataset(string name)
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
            }, _cancellationTokenSource.Token));
        }

        public async Task<IEnumerable<string>> ListDatasets()
        {
            return await ClientCall(new Task<IEnumerable<string>>(() =>
            {
                var datasetList = _brightstarClient.ListStores() ?? new List<string>();
                return datasetList;
            }, _cancellationTokenSource.Token));
        }

        public async Task<bool> DeleteGraph(string dataset, string graphUri)
        {
            return await ClientCall(new Task<bool>(() =>
            {
                var response = _httpClient.DeleteAsync($"{_endpointUri}/{dataset}/{graphUri}", _cancellationTokenSource.Token).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Log.Debug($"Error {response.StatusCode} while sending HTTP request to {_endpointUri}: {response.Content}");
                    return false;
                }

                return true;
            }));
        }

        public async Task<bool> UpdateGraph(string dataset, string graphUri, IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)
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

        public async Task<IGraph> ReadGraph(string dataset, string graphUri)
        {
            return await ClientCall(new Task<IGraph>(() =>
            {
                var resultGraph = new Graph();
                using (var connector = new SparqlConnector(new Uri($"{_endpointUri}/{dataset}//SPARQL")))
                {
                    connector.Timeout = 5000; //5s
                    connector.LoadGraph(resultGraph, graphUri);
                    return resultGraph;
                }
            }));
        }

        public async Task<IEnumerable<string>> ListGraphs(string dataset)
        {
            return await ClientCall(new Task<IEnumerable<string>>(() => _brightstarClient.ListNamedGraphs(dataset) ?? new List<string>()));
        }

        public async Task<SparqlResultSet> RunSparqlQuery(IEnumerable<string> graphUris, string query)
        {
            return await ClientCall(new Task<SparqlResultSet>(() =>
            {
                var resultGraph = new Graph();
                var endpoint = new SparqlRemoteEndpoint(new Uri(_endpointUri), graphUris);
                return endpoint.QueryWithResultSet(query);
            }));
        }
        #endregion

        protected override void OnDispose(bool disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _httpClient.Dispose();
            _brightstarClient = null;

            Log.Debug("BrightstarClient disposed");
        }

        //TODO add timeout logic
        private async Task<T> ClientCall<T>(Task<T> call)
        {
            try
            {
                return await call;
            }
            catch (WebException)
            {
                Log.Debug($"Request to {_endpointUri} failed due to the connection error");
                return default(T);
            }
            catch (TaskCanceledException)
            {
                return default(T);
            }
            catch (Exception e)
            {
                Log.Error($"Request to {_endpointUri} failed: {e.GetDetails()}");
                return default(T);
            }
        }
    }
}
