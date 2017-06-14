using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using static Serilog.Log;

namespace GraphDataRepository.Server
{
    /// <summary>
    /// Base class for all triplestores
    /// </summary>
    public abstract class TriplestoreClient : Disposable, ITriplestoreClient
    {
        protected readonly string EndpointUri;
        protected readonly List<Task> CallTasks = new List<Task>();
        protected readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        protected readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        protected TriplestoreClient(string endpoint)
        {
            EndpointUri = endpoint;
        }

        public abstract Task<bool> CreateDataset(string name);
        public abstract Task<bool> DeleteDataset(string name);
        public abstract Task<IEnumerable<string>> ListDatasets();
        public abstract Task<bool> UpdateGraph(string dataset, Uri graphUri, IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd);

        public async Task<IEnumerable<Uri>> ListGraphs(string dataset)
        {
            return await ClientCall(Task.Run(() =>
            {
                using (var connector = new SparqlConnector(new Uri($"{EndpointUri}/{dataset}/SPARQL")))
                {
                    using (var store = new PersistentTripleStore(connector))
                    {
                        return store.UnderlyingStore.ListGraphs();
                    }
                }
            }, CancellationTokenSource.Token));
        }

        public async Task<bool> DeleteGraphs(string dataset, IEnumerable<Uri> graphUris)
        {
            return await ClientCall(Task.Run(() =>
            {
                foreach (var uri in graphUris)
                {
                    var response = HttpClient.DeleteAsync($"{EndpointUri}/{dataset}/graphs?graph={uri}", CancellationTokenSource.Token).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug($"Error {response.StatusCode} while sending HTTP request to {EndpointUri}: {response.Content}." +
                              $"Graph {uri} not deleted.");
                        return false;
                    }
                }

                return true;
            }, CancellationTokenSource.Token));
        }

        public async Task<IEnumerable<IGraph>> ReadGraphs(string dataset, IEnumerable<Uri> graphUris)
        {
            return await ClientCall(Task.Run(() =>
            {
                var resultGraphs = new List<IGraph>();
                using (var connector = new SparqlConnector(new Uri($"{EndpointUri}/{dataset}/SPARQL")))
                {
                    foreach (var uri in graphUris)
                    {
                        var graph = new Graph();
                        connector.LoadGraph(graph, uri);
                        resultGraphs.Add(graph);
                    }
                }

                return resultGraphs;
            }, CancellationTokenSource.Token));
        }

        public async Task<SparqlResultSet> RunSparqlQuery(string dataset, IEnumerable<Uri> graphs, string query)
        {
            return await ClientCall(Task.Run(() =>
            {
                var endpoint = new SparqlRemoteEndpoint(new Uri($"{EndpointUri}/{dataset}/SPARQL"), graphs);
                using (var connector = new SparqlConnector(endpoint))
                {
                    using (var store = new PersistentTripleStore(connector))
                    {
                        return store.ExecuteQuery(query) as SparqlResultSet;
                    }
                }
            }, CancellationTokenSource.Token));
        }

        protected async Task<T> ClientCall<T>(Task<T> call)
        {
            lock (CallTasks)
            {
                var finishedTasks = CallTasks.Where(t => t.IsCompleted || t.IsFaulted || t.IsCanceled).ToArray();
                foreach (var finishedTask in finishedTasks)
                {
                    CallTasks.Remove(finishedTask);
                }

                CallTasks.Add(call);
            }

            try
            {
                return await call;
            }
            catch (WebException webEx)
            {
                Debug($"{webEx.GetDetails()}");
                return default(T);
            }
            catch (TaskCanceledException)
            {
                return default(T);
            }
            catch (Exception e)
            {
                Error($"Request to {EndpointUri} failed: {e.GetDetails()}");
                return default(T);
            }
        }

        protected override void OnDispose(bool disposing)
        {
            CancellationTokenSource.Cancel();

            Task[] tasksToWait;
            lock (CallTasks)
            {
                tasksToWait = CallTasks.ToArray();
            }

            if (!Task.WaitAll(tasksToWait, TimeSpan.FromSeconds(10)))
            {
                Error("Client call tasks did not finish in specified time");
            }

            CancellationTokenSource.Dispose();
            HttpClient.Dispose();
        }
    }
}
