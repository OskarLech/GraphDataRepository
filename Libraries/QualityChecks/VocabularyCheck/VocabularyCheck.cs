using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using VDS.RDF;
using VDS.RDF.Parsing;
using static Serilog.Log;

namespace Libraries.QualityChecks.VocabularyCheck
{
    /// <summary>
    /// Checks if predicates used in triples are defined in vocabularies.
    /// Parameters for this class are vocabulary URIs that can point either to file on a disk or web resource
    /// </summary>
    public class VocabularyCheck : QualityCheck
    {
        #region public methods

        public override QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters)
        {
            IsCheckInProgress = true;
            var parameterList = parameters.ToList(); //multiple enumeration
            if (!AreParametersSupported(parameterList))
            {
                return null;
            }

            var vocabularyUris = ParseParameters<Uri>(parameterList);
            var predicateList = GetPredicates(vocabularyUris);

            var wrongTriples = new List<(Uri GraphUri, string Triple)>();
            try
            {
                Parallel.ForEach(graphs, ParallelOptions, graph =>
                {
                    lock (wrongTriples)
                    {
                        var wrongTriplesInGraph = CheckTriples(graph.Triples.Select(t => t.ToString()).ToList(), predicateList);
                        wrongTriples.AddRange(wrongTriplesInGraph.Select(triple => (graph.BaseUri, triple)));
                    }
                });
            }
            catch (OperationCanceledException)
            {
                IsCheckInProgress = false;
                return null;
            }

            return GenerateQualityCheckReport(wrongTriples);
        }

        public override QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<object> parameters)
        {
            IsCheckInProgress = true;

            var parameterList = parameters.ToList(); //multiple enumeration
            if (!AreParametersSupported(parameterList))
            {
                return null;
            }

            var vocabularyUris = ParseParameters<Uri>(parameterList);
            var predicateList = GetPredicates(vocabularyUris);

            var wrongTriples = CheckTriples(triples, predicateList);
            return GenerateQualityCheckReport(wrongTriples.Select(t => new ValueTuple<Uri, string>(null, t)).ToList());
        }

        #endregion

        #region private methods

        private IEnumerable<string> CheckTriples(IEnumerable<string> triples, IEnumerable<string> predicateList)
        {
            var wrongTriples = new List<string>();
            try
            {
                Parallel.ForEach(triples, ParallelOptions, triple =>
                {
                    var predicate = triple.Predicate();
                    if (predicateList.Contains(predicate)) return;

                    lock (wrongTriples)
                    {
                        if (wrongTriples.Contains(triple)) return;
                        Verbose($"Predicate not found in any dictionary: {triple.Predicate()}");
                        wrongTriples.Add(triple);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                IsCheckInProgress = false;
                return null;
            }

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

        private QualityCheckReport GenerateQualityCheckReport(IReadOnlyCollection<(Uri GraphUri, string Triple)> wrongTriples)
        {
            var report = new QualityCheckReport();

            if (!wrongTriples.Any())
            {
                report.QualityCheckPassed = true;
                return report;
            }

            var errorId = 1;
            foreach (var graphTriple in wrongTriples)
            {
                var graphUri = graphTriple.GraphUri != null
                    ? graphTriple.GraphUri.ToString()
                    : "Default graph or triple not bound to graph";

                report.ErrorsById[errorId] = (graphUri, graphTriple.Triple, $"Predicate not found in any dictionary: {graphTriple.Triple.Predicate()}");
                errorId++;
            }

            IsCheckInProgress = false;
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
            //TODO
            throw new NotImplementedException();
        }

        #endregion
    }
}
