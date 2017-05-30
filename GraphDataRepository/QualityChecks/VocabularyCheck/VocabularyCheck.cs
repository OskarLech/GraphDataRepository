using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using VDS.RDF;
using VDS.RDF.Parsing;
using static Serilog.Log;

namespace GraphDataRepository.QualityChecks.VocabularyCheck
{
    /// <summary>
    /// Checks if predicates used in triples are defined in vocabularies.
    /// </summary>
    public class VocabularyCheck : QualityCheck
    {
        #region public methods

        public override QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters)
        {
            var parameterList = parameters as IList<object> ?? parameters.ToList(); //multiple enumeration
            if (!AreParametersSupported(parameterList))
            {
                return null;
            }

            var vocabularyUris = ParseParameters<Uri>(parameterList);
            var predicateList = GetPredicates(vocabularyUris);

            var wrongTriples = new List<Triple>();
            var parallelOptions = new ParallelOptions { CancellationToken = CancellationTokenSource.Token };
            Parallel.ForEach(graphs, parallelOptions, graph =>
            {
                lock (wrongTriples)
                {
                    wrongTriples.AddRange(CheckTriples(graph.Triples.ToList(), predicateList));
                }
            });

            return GenerateQualityCheckReport(wrongTriples);
        }

        public override QualityCheckReport CheckData(IEnumerable<Triple> triples, IEnumerable<object> parameters, IEnumerable<IGraph> graphs = null)
        {
            if (graphs != null)
            {
                throw new Exception($"{GetType().Name} quality check cannot compare triples against graphs.");
            }

            var parameterList = parameters as IList<object> ?? parameters.ToList(); //multiple enumeration
            if (!AreParametersSupported(parameterList))
            {
                return null;
            }

            var vocabularyUris = ParseParameters<Uri>(parameterList);
            var predicateList = GetPredicates(vocabularyUris);

            var wrongTriples = CheckTriples(triples, predicateList);
            return GenerateQualityCheckReport(wrongTriples.ToList());
        }

        public override bool ImportParameters(IEnumerable<object> parameters)
        {
            throw new NotImplementedException();
        }

        public override void FixErrors(QualityCheckReport qualityCheckReport, string dataset, IEnumerable<int> errorsToFix)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region private methods

        private IEnumerable<Triple> CheckTriples(IEnumerable<Triple> triples, IEnumerable<string> predicateList)
        {
            var wrongTriples = new List<Triple>();
            var parallelOptions = new ParallelOptions { CancellationToken = CancellationTokenSource.Token };
            Parallel.ForEach(triples, parallelOptions, triple =>
            {
                var predicate = triple.Predicate.ToString();
                if (predicateList.Contains(predicate)) return;

                lock (wrongTriples)
                {
                    if (wrongTriples.Contains(triple)) return;
                    Logger.Verbose($"Predicate not found in any dictionary: {triple.Predicate}");
                    wrongTriples.Add(triple);
                }
            });

            return wrongTriples;
        }

        private IEnumerable<string> GetPredicates(IEnumerable<Uri> vocabularyUris)
        {
            var vocabularySubjectsByUri = new Dictionary<Uri, IEnumerable<string>>();
            foreach (var vocabularyUri in vocabularyUris)
            {
                vocabularySubjectsByUri[vocabularyUri] = LoadVocabulary(vocabularyUri);
            }

            return vocabularySubjectsByUri.SelectMany(predicate => predicate.Value).Distinct().ToList();
        }

        private static QualityCheckReport GenerateQualityCheckReport(List<Triple> wrongTriples)
        {
            var report = new QualityCheckReport();

            if (!wrongTriples.Any())
            {
                report.QualityCheckPassed = true;
                return report;
            }

            var errorId = 1;
            foreach (var triple in wrongTriples)
            {
                var graphUri = triple.GraphUri != null
                    ? triple.GraphUri.ToString()
                    : "Default graph";

                report.ErrorsById[errorId] = new Tuple<string, string, string, bool>
                    (graphUri, triple.PrettyPrint(), $"Predicate not found in any dictionary: {triple.Predicate}", false);

                errorId++;
            }

            return report;
        }

        private IEnumerable<string> LoadVocabulary(Uri vocabularyUri)
        {
            var vocabularyFilePath = !vocabularyUri.IsFile 
                ? DownloadVocabulary(vocabularyUri) 
                : vocabularyUri.AbsolutePath;

            var schemaGraph = new Graph();
            FileLoader.Load(schemaGraph, vocabularyFilePath);
            return schemaGraph.Triples.Select(triple => triple.Subject.ToString()).Distinct().ToList();
        }

        private string DownloadVocabulary(Uri vocabularyUri)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
