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
    /// Parameters for this class are URIs of endpoint and default graph (second one can be null) and filter (can be null as well)
    /// </summary>
    public class KnowledgeBaseCheck : QualityCheck
    {
        public override QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters)
        {
            var parameterList = parameters as IList<object> ?? parameters.ToList(); //multiple enumeration
            if (!AreParametersSupported(parameterList))
            {
                return null;
            }

            var parsedParameters = ParseParameters<(Uri endpointUri, Uri graphUri, string filter)>(parameterList);
            var checksFailed = new List<(Uri, Uri, string)>();

            try
            {
                Parallel.ForEach(parsedParameters, ParallelOptions, parameter =>
                {
                    var endpoint = new SparqlRemoteEndpoint(parameter.endpointUri, parameter.graphUri);
                    SparqlResultSet results = null;
                    try
                    {
                        results = endpoint.QueryWithResultSet(parameter.Item3);
                    }
                    catch (Exception e)
                    {
                        Error($"{GetType().Name} quality check failed for endpoint {parameter.endpointUri}" +
                                     (parameter.graphUri != null ? $"(graph {parameter.graphUri})" : "(default graph)") +
                                     $":\n{e.GetDetails()}");
                    }

                    if (results != null && results.Any())
                    {
                        foreach (var result in results)
                        {
                            Verbose($"{result}");
                        }
                    }
                    else
                    {
                        lock (checksFailed)
                        {
                            checksFailed.Add(parameter);
                        }
                    }
                });
            }
            catch (OperationCanceledException)
            {
                IsCheckInProgress = false;
                return null;
            }

            return GenerateQualityCheckReport(checksFailed);
        }

        public override QualityCheckReport CheckData(IEnumerable<Triple> triples, IEnumerable<object> parameters, IEnumerable<IGraph> graphs = null)
        {
            throw new System.NotImplementedException();
        }

        public override void FixErrors(QualityCheckReport qualityCheckReport, string dataset, IEnumerable<int> errorsToFix)
        {
            throw new System.NotImplementedException();
        }

        public override bool ImportParameters(IEnumerable<object> parameters)
        {
            throw new System.NotImplementedException();
        }

        private QualityCheckReport GenerateQualityCheckReport(IEnumerable<(Uri, Uri, string)> failedQueries)
        {
            var report = new QualityCheckReport();

            if (!failedQueries.Any())
            {
                report.QualityCheckPassed = true;
                return report;
            }

            //var errorId = 1;
            //foreach (var query in failedQueries)
            //{
            //    report.ErrorsById[errorId] = new Tuple<string, string, string, bool>
            //    (query.Item1.ToString(), triple.PrettyPrint(), $"Predicate not found in any dictionary: {triple.Predicate}",
            //        false);

            //    errorId++;
            //}

            IsCheckInProgress = false;
            return report;
        }
    }
}
