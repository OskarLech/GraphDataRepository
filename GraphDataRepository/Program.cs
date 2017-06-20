using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Libraries.QualityChecks.KnowledgeBaseCheck;
using Libraries.QualityChecks.VocabularyCheck;
using Libraries.Server;
using Serilog;
using VDS.RDF;
using VDS.RDF.Parsing;
using static Serilog.Log;

namespace GraphDataRepository
{
    internal class Program
    {
        private static readonly List<IDisposable> Disposables = new List<IDisposable>();

        private static void Main()
        {
            Initialize();

            //BrightstarClientCli();
            TestQualityChecks();

            /*******************************************************/

            var mre = new ManualResetEvent(false);
            mre.WaitOne();

            Console.WriteLine("Press Enter to stop the program");
            Console.ReadLine();

            Verbose("Terminating the program");
            Disposables.ForEach(d => d.Dispose());
            Verbose("Program terminated succesfully");
        }

        private static void TestQualityChecks()
        {
            var dataGraph = new Graph();
            FileLoader.Load(dataGraph, @"..\..\..\Common\TestData\RDF\foaf_example.rdf");

            //var query = "SELECT DISTINCT ?concept\r\nWHERE {\r\n    <http://dbpedia.org/resource/NASA> a ?concept\r\n    FILTER ( strstarts(str(?concept), \"http://dbpedia.org/class/yago/\") )\r\n}\r\nLIMIT 1";
            var knowledgeBaseCheck = new KnowledgeBaseCheck();
            var parameters = (object) new ValueTuple<Uri, Uri, string>(new Uri("http://dbpedia.org/sparql"), null, "");
            knowledgeBaseCheck.CheckGraphs(dataGraph.AsEnumerable(), parameters.AsEnumerable());

            //knowledgeBaseCheck.CheckGraphs(dataGraph.AsEnumerable(), TupleExtensions.ToTuple<Uri, Uri, string>((new Uri("http://dbpedia.org/sparql"), null, query)).AsEnumerable());

            var vocabPath = Path.GetFullPath(@"..\..\..\Common\TestData\Schemas\foaf_20140114.rdf");
            var vocabCheck = new VocabularyCheck();
            vocabCheck.CheckGraphs(dataGraph.AsEnumerable(), new Uri(vocabPath).AsEnumerable());
        }
        
        private static void Initialize()
        {
            //Serilog
            Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .CreateLogger();
        }
    }
}
