using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using VDS.RDF;
using VDS.RDF.Query;
using static Serilog.Log;

namespace Libraries.QualityChecks.KnowledgeBaseCheck
{
    /// <summary> 
    /// Checks whether triple subjects exist in external knowledge base. Allows for applying strstarts(...) filter on results.
    /// Example: Checking if subject is in DBpedia knowledge base and if it's concept is known in YAGO project.
    /// Parameters for this class are URIs of endpoint and one of its graphs (default graph if empty) and filter (can be empty)
    /// </summary>
    public class KnowledgeBaseCheck : QualityCheck
    {
        private const string BaseQuery = "SELECT DISTINCT ?concept WHERE { <{subject}> a ?concept {filter} } LIMIT 1";
        private const string Filter = "FILTER ( strstarts(str(?concept), \"{filter}\") )";

        public override QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters)
        {
            IsCheckInProgress = true;
            var parameterList = parameters.ToList(); //multiple enumeration
            if (!AreParametersValid(parameterList))
            {
                return null;
            }

            var parsedParameters = ParseParameters(parameterList);

            var triplesList = graphs.SelectMany(g => g.Triples).Distinct().Select(t => t.ToString()).ToList();
            var subjectList = triplesList.Select(t => t.Subject()).Distinct().ToList();
            var failedQueries = CheckSubjects(parsedParameters, subjectList);

            return GenerateQualityCheckReport(triplesList, failedQueries);
        }

        public override QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<object> parameters)
        {
            IsCheckInProgress = true;

            //multiple enumerations
            var triplesList = triples.ToList();
            var parameterList = parameters.ToList();

            if (!AreParametersValid(parameterList))
            {
                return null;
            }

            var parsedParameters = ParseParameters(parameterList);
            if (parsedParameters == null) return null;

            var subjectList = triplesList.Select(t => t.Subject()).Distinct().ToList();
            var failedQueries = CheckSubjects(parsedParameters, subjectList);

            return GenerateQualityCheckReport(triplesList, failedQueries);
        }
        
        private IEnumerable<string> CheckSubjects(IEnumerable<(Uri endpointUri, Uri graphUri, string filter)> parsedParameters, IReadOnlyCollection<string> subjectList)
        {
            var passedSubjects = new List<string>();

            try
            {
                var loopResult = Parallel.ForEach(parsedParameters, ParallelOptions, (parameter, state) =>
                {
                    var endpoint = new SparqlRemoteEndpoint(parameter.endpointUri, parameter.graphUri);
                    try
                    {
                        foreach (var subject in subjectList)
                        {
                            var query = BaseQuery.Replace("{subject}", subject);

                            var filterReplacement = "";
                            if (!string.IsNullOrWhiteSpace(parameter.filter))
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

                                lock (passedSubjects)
                                {
                                    passedSubjects.Add(subject);
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
                    return null;
                }
            }
            catch (OperationCanceledException)
            {
                IsCheckInProgress = false;
                return null;
            }

            return subjectList.Except(passedSubjects.Distinct());
        }

        private QualityCheckReport GenerateQualityCheckReport(IEnumerable<string> triples, IEnumerable<string> failedSubjects)
        {
            if (failedSubjects == null) return null;

            var report = new QualityCheckReport();

            var failedSubjectsList = failedSubjects.ToList();
            if (!failedSubjectsList.Any())
            {
                report.QualityCheckPassed = true;
                return report;
            }

            var errorId = 1;
            var triplesList = triples.ToList(); //multiple enumeration
            foreach (var subject in failedSubjectsList)
            {
                foreach (var triple in triplesList.Where(t => t.Subject() == subject))
                {
                    var errorMsg = $"Query for subject {subject} returned no results in any of specified endpoints.";
                    report.ErrorsById[errorId] = ("", triple, errorMsg);
                }

                errorId++;
            }

            IsCheckInProgress = false;
            return report;
        }

        protected IEnumerable<(Uri endpointUri, Uri graphUri, string filter)> ParseParameters(IEnumerable<object> parameters)
        {
            try
            {
                var parameterStrings = parameters.Select(p => p.ToString().Split(new[] { "," }, StringSplitOptions.None));

                return (from stringArray in parameterStrings
                        let endpointUri = new Uri(stringArray[0])
                        let graphUri = string.IsNullOrEmpty(stringArray[1]) || stringArray[1] == "default" 
                            ? null 
                            : new Uri(stringArray[1])
                        select (endpointUri, graphUri, stringArray[2])).ToList();
            }
            catch (Exception e)
            {
                Error($"Cannot convert parameters to {typeof((Uri endpointUri, Uri graphUri, string filter))}: {e.GetDetails()}");
                return null;
            }
        }
    }
}
