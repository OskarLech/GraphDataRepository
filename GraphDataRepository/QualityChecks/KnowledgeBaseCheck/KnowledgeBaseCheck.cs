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
    /// Checks whether triple subjects exist in external knowledge base and if custom SPARQL query for them has results.
    /// Example: Checking if subject is in DBpedia knowledge base and if it's concept is known in YAGO project.
    /// </summary>
    public class KnowledgeBaseCheck : QualityCheck
    {
        public override QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters)
        {
            var parameterList = parameters as IList<object> ?? parameters.ToList(); //multiple enumeration
            if (!AreParametersSupported(parameterList)) return null;
            var parsedParameters = ParseParameters<Tuple<Uri, Uri, string>>(parameterList);

            var parallelOptions = new ParallelOptions {CancellationToken = CancellationTokenSource.Token};
            Parallel.ForEach(parsedParameters, parallelOptions, parameter =>
            {
                var endpoint = new SparqlRemoteEndpoint(parameter.Item1, parameter.Item2);
                SparqlResultSet results = null;
                try
                {
                    results = endpoint.QueryWithResultSet(parameter.Item3);
                }
                catch (Exception e)
                {
                    Logger.Error($"{GetType().Name} quality check failed for endpoint {parameter.Item1}" +
                                 (parameter.Item2 != null ? $"(graph {parameter.Item2})" : "(default graph)") +
                                 $"{e.GetDetails()}");
                }

                if (results != null && results.Count > 0)
                {
                    foreach (var result in results)
                    {
                        Logger.Verbose($"{result}");
                    }
                }
            });

            return null;
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
    }
}
