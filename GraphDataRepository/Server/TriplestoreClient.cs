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
            Timeout = TimeSpan.FromSeconds(5) //TODO add settings
        };

        protected TriplestoreClient(ILog log, string endpoint) : base(log)
        {
            EndpointUri = endpoint;
        }

        public abstract Task<bool> CreateDataset(string name);
        public abstract Task<bool> DeleteDataset(string name);
        public abstract Task<IEnumerable<string>> ListDatasets();
        public abstract Task<bool> UpdateGraph(string dataset, string graphUri, IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd);
        public abstract Task<IEnumerable<string>> ListGraphs(string dataset);

        public virtual async Task<bool> DeleteGraph(string dataset, string graphUri)
        {
            return await ClientCall(new Task<bool>(() =>
            {
                var response = HttpClient.DeleteAsync($"{EndpointUri}/{dataset}/{graphUri}", CancellationTokenSource.Token).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Log.Debug($"Error {response.StatusCode} while sending HTTP request to {EndpointUri}: {response.Content}");
                    return false;
                }

                return true;
            }));
        }

        public virtual async Task<IGraph> ReadGraph(string dataset, string graphUri)
        {
            return await ClientCall(new Task<IGraph>(() =>
            {
                var resultGraph = new Graph();
                using (var connector = new SparqlConnector(new Uri($"{EndpointUri}/{dataset}//SPARQL")))
                {
                    connector.Timeout = 5000; //5s
                    connector.LoadGraph(resultGraph, graphUri);
                    return resultGraph;
                }
            }));
        }

        public virtual async Task<SparqlResultSet> RunSparqlQuery(IEnumerable<string> graphUris, string query)
        {
            return await ClientCall(new Task<SparqlResultSet>(() =>
            {
                var endpoint = new SparqlRemoteEndpoint(new Uri(EndpointUri), graphUris);
                return endpoint.QueryWithResultSet(query);
            }));
        }

        //TODO add timeout logic
        protected async Task<T> ClientCall<T>(Task<T> call)
        {
            try
            {
                return await call;
            }
            catch (WebException)
            {
                Log.Debug($"Request to {EndpointUri} failed due to the connection error");
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
