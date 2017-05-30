using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace GraphDataRepository.QualityChecks.VocabularyCheck
{
    /// <summary>
    /// Checks if predicates used in triples are defined in vocabularies.
    /// </summary>
    public class VocabularyCheck : QualityCheck
    {
        private readonly Dictionary<Uri, IEnumerable<string>> _vocabularySubjectsByUri = new Dictionary<Uri, IEnumerable<string>>(); //TODO: temp for POC 

        public VocabularyCheck()
        {
            LoadVocabsTemp();
        }

        private void LoadVocabsTemp() //TODO: temp for POC
        {
            var schemaGraph = new Graph();
            FileLoader.Load(schemaGraph, @"..\..\..\Common\TestData\Schemas\foaf_20140114.rdf");
            var subjectList = schemaGraph.Triples.Select(triple => triple.Subject.ToString()).Distinct().ToList();
            _vocabularySubjectsByUri[new Uri("http://xmlns.com/foaf/spec/")] = subjectList;
        }

        public override QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters = null)
        {
            var parsedParameters = ParseParameters(typeof(Uri), parameters);

            var predicateList = _vocabularySubjectsByUri[new Uri("http://xmlns.com/foaf/spec/")].ToList();//TODO: make list from all vocabs passed in parameters
            var wrongPredicates = new List<string>();

            //TODO parallel foreaches
            foreach (var graph in graphs)
            {
                foreach (var triple in graph.Triples)
                {
                    var predicate = triple.Predicate.ToString();
                    if (!predicateList.Contains(predicate) && !wrongPredicates.Contains(predicate))
                    {
                        wrongPredicates.Add(predicate);
                    }
                }
            }

            foreach (var predicate in wrongPredicates)
            {
                Console.WriteLine(predicate);
            }

            return new QualityCheckReport(); //TODO
        }

        public override QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<IGraph> graphs = null, IEnumerable<object> parameters = null)
        {
            var parsedParameters = ParseParameters(typeof(Uri), parameters);
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
