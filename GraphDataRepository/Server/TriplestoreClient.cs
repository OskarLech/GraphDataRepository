using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common;
using log4net;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace GraphDataRepository.Server
{
    /// <summary>
    /// Base class for all triplestores
    /// </summary>
    public abstract class TriplestoreClient : Disposable, ITriplestoreClient
    {
        protected readonly string EndpointUri;
        protected readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        protected readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        protected TriplestoreClient(ILog log, string endpoint) : base(log)
        {
            EndpointUri = endpoint;
        }

        public abstract Task<bool> CreateDataset(string name);
        public abstract Task<bool> DeleteDataset(string name);
        public abstract Task<IEnumerable<string>> ListDatasets();
        public abstract Task<bool> UpdateGraph(string dataset, string graphUri, IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd);

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

        public async Task<bool> DeleteGraph(string dataset, string graphUri)
        {
            return await ClientCall(Task.Run(() =>
            {
                var response = HttpClient.DeleteAsync($"{EndpointUri}/{dataset}/graphs?graph={graphUri}", CancellationTokenSource.Token).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Log.Debug($"Error {response.StatusCode} while sending HTTP request to {EndpointUri}: {response.Content}");
                    return false;
                }

                return true;
            }, CancellationTokenSource.Token));
        }

        public async Task<IGraph> ReadGraph(string dataset, string graphUri)
        {
            return await ClientCall(Task.Run(() =>
            {
                Graph resultGraph;
                using (var connector = new SparqlConnector(new Uri($"{EndpointUri}/{dataset}/SPARQL")))
                {
                    resultGraph = new Graph();
                    connector.LoadGraph(resultGraph, graphUri);
                }

                return resultGraph;
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
            try
            {
                return await call;
            }
            catch (WebException webEx)
            {
                Log.Debug($"{webEx.GetDetails()}");
                return default(T);
            }
            catch (TaskCanceledException)
            {
                return default(T);
            }
            catch (Exception e)
            {
                Log.Error($"Request to {EndpointUri} failed: {e.GetDetails()}");
                return default(T);
            }
        }
    }
}
