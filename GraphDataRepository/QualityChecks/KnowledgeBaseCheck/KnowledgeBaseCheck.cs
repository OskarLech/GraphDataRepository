using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using VDS.RDF;
using VDS.RDF.Query;
using static Serilog.Log;

namespace GraphDataRepository.QualityChecks.KnowledgeBaseCheck
{
    /// <summary> 
    /// Checks whether triple subjects exist in external knowledge base. Allows for applying strstarts(...) filter on results.
    /// Example: Checking if subject is in DBpedia knowledge base and if it's concept is known in YAGO project.
    /// Parameters for this class are URIs of endpoint and one of its graphs (default graph if empty) and filter (can be empty)
    /// </summary>
    public class KnowledgeBaseCheck : QualityCheck
    {
        private const string BaseQuery = "SELECT DISTINCT ?concept WHERE { \"{subject}\" a ?concept {filter} } LIMIT 1";
        private const string Filter = "FILTER ( strstarts(str(?concept), \"{filter}\") )";

        public override QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters)
        {
            IsCheckInProgress = true;
            var parameterList = parameters.ToList(); //multiple enumeration
            if (!AreParametersSupported(parameterList))
            {
                return null;
            }

            var parsedParameters = ParseParameters<(Uri endpointUri, Uri graphUri, string filter)>(parameterList);

            var triplesList = graphs.SelectMany(g => g.Triples).Distinct().ToList();
            var subjectList = triplesList.Select(t => t.Subject.ToString()).Distinct().ToList();
            var failedQueries = CheckSubjects(parsedParameters, subjectList);

            return GenerateQualityCheckReport(triplesList, failedQueries);
        }

        public override QualityCheckReport CheckData(IEnumerable<Triple> triples, IEnumerable<object> parameters)
        {
            IsCheckInProgress = true;

            //multiple enumerations
            var triplesList = triples.ToList();
            var parameterList = parameters.ToList();

            if (!AreParametersSupported(parameterList))
            {
                return null;
            }

            var parsedParameters = ParseParameters<(Uri endpointUri, Uri graphUri, string filter)>(parameterList);

            var subjectList = triplesList.Select(t => t.Subject.ToString()).Distinct().ToList();
            var failedQueries = CheckSubjects(parsedParameters, subjectList);

            return GenerateQualityCheckReport(triplesList, failedQueries);
        }

        public override bool ImportParameters(IEnumerable<object> parameters)
        {
            throw new System.NotImplementedException();
        }

        private Dictionary<string, ValueTuple<Uri, Uri, string>> CheckSubjects(IEnumerable<(Uri endpointUri, Uri graphUri, string filter)> parsedParameters, IReadOnlyCollection<string> subjectList)
        {
            var failedQueries = new Dictionary<string, (Uri endpointUri, Uri graphUri, string filter)>();

            try
            {
                var loopResult = Parallel.ForEach(parsedParameters, ParallelOptions, (parameter, state) =>
                {
                    var endpoint = new SparqlRemoteEndpoint(parameter.endpointUri, parameter.graphUri);
                    try
                    {
                        foreach (var subject in subjectList)
                        {
                            //TODO injection protection
                            var query = BaseQuery.Replace("{subject}", subject);

                            var filterReplacement = "";
                            if (!string.IsNullOrEmpty(parameter.filter))
                            {
                                filterReplacement = Filter.Replace("{filter}", parameter.filter);
                            }

                            query = query.Replace("{filter}", filterReplacement);

                            var results = endpoint.QueryWithResultSet(query);
                            if (results != null && results.Any())
                            {
                                foreach (var result in results)
                                {
                                    Verbose($"{result}");
                                }
                            }
                            else
                            {
                                lock (failedQueries)
                                {
                                    failedQueries.Add(subject, parameter);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Error($"{GetType().Name} quality check error for endpoint {parameter.endpointUri}" +
                              (parameter.graphUri != null ? $"(graph {parameter.graphUri})" : "(default graph)") +
                              $":\n{e.GetDetails()}");

                        state.Stop();
                    }
                });

                if (!loopResult.IsCompleted)
                {
                    IsCheckInProgress = false;
                    return failedQueries;
                }
            }
            catch (OperationCanceledException)
            {
                IsCheckInProgress = false;
                return failedQueries;
            }

            return failedQueries;
        }

        private QualityCheckReport GenerateQualityCheckReport(IEnumerable<Triple> triples, Dictionary<string, (Uri endpointUri, Uri graphUri, string filter)> failedQueries)
        {
            var report = new QualityCheckReport();

            if (!failedQueries.Any())
            {
                report.QualityCheckPassed = true;
                return report;
            }

            var errorId = 1;
            var triplesList = triples.ToList(); //multiple enumeration
            foreach (var query in failedQueries)
            {
                foreach (var triple in triplesList.Where(t => t.Subject.ToString() == query.Key))
                {
                    var graphUriMsg = query.Value.graphUri != null
                        ? query.Value.graphUri.ToString()
                        : "Default graph";

                    var errorMsg = $"Query for subject {query.Key} to {query.Value.endpointUri} ({graphUriMsg}) with filter {query.Value.filter} returned no results.";
                    report.ErrorsById[errorId] = (triple.GraphUri.ToString(), triple.Print(), errorMsg);
                }

                errorId++;
            }

            IsCheckInProgress = false;
            return report;
        }
    }
}
