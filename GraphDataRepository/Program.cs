using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using BrightstarDB;
using BrightstarDB.Client;
using GraphDataRepository.QualityChecks.KnowledgeBaseCheck;
using GraphDataRepository.QualityChecks.VocabularyCheck;
using GraphDataRepository.Server.BrightstarDb;
using GraphDataRepository.Utilities.StructureMap;
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

        private static async void BrightstarClientCli()
        {
            var triplestoreClient = ObjectFactory.Container
                .With("endpoint").EqualTo("http://192.168.0.111:8090/brightstar")
                .GetInstance<IBrightstarClient>();

            Disposables.Add(triplestoreClient);

            while (true)
            {
                Console.WriteLine("\n\nESC - exit");
                Console.WriteLine("F1 - Create dataset");
                Console.WriteLine("C - Delete dataset");
                Console.WriteLine("F3 - List datasets");
                Console.WriteLine("F4 - Delete graph");
                Console.WriteLine("F5 - Update graph");
                Console.WriteLine("F6 - Read graph");
                Console.WriteLine("F7 - List graphs");
                Console.WriteLine("F8 - Run SPARQL query\n\n");

                string dataset;
                string graph;
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.F1:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        if (await triplestoreClient.CreateDataset(dataset))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.C:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        if (await triplestoreClient.DeleteDataset(dataset))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.F3:
                        foreach (var ds in await triplestoreClient.ListDatasets())
                        {
                            Console.WriteLine(ds);
                        }
                        break;

                    case ConsoleKey.F4:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("graph URI:");
                        graph = Console.ReadLine();
                        if (await triplestoreClient.DeleteGraph(dataset, new Uri(graph)))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.F5: //update graph
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("graph URI:");
                        graph = Console.ReadLine();

                        var triplesToRemove = new List<string>();
                        var triplesToAdd = new List<string>();

                        triplesToRemove.Add("<xhttp://www.w3.org/2001/sw/RDFCore/ntriples/> <xhttp://www.w3.org/1999/02/22-rdf-syntax-ns#type> <xhttp://xmlns.com/foaf/0.1/Document>");
                        triplesToRemove.Add("<xhttp://www.w3.org/2001/sw/RDFCore/ntriples/> <xhttp://www.w3.org/1999/02/22-rdf-syntax-ns#type> <xhttp://xmlns.com/foaf/0.1/Document>");
                        triplesToRemove.Add("<xhttp://www.w3.org/2001/sw/RDFCore/ntriples/> <xhttp://www.w3.org/1999/02/22-rdf-syntax-ns#type> <xhttp://xmlns.com/foaf/0.1/Document>");
                        triplesToAdd.Add("<http://www.w3.org/2001/sw/RDFCore/ntriples/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/Document>");
                        triplesToAdd.Add("<http://www.w3.org/2001/sw/RDFCore/ntriples/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/Document>");
                        triplesToAdd.Add("<http://www.w3.org/2001/sw/RDFCore/ntriples/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/Document>");

                        if (await triplestoreClient.UpdateGraph(dataset, new Uri(graph), triplesToRemove, triplesToAdd))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.F6:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("graph URI:");
                        graph = Console.ReadLine();

                        var dnrGraph = await triplestoreClient.ReadGraph(dataset, new Uri(graph));
                        break;

                    case ConsoleKey.F7:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        foreach (var g in await triplestoreClient.ListGraphs(dataset))
                        {
                            Console.WriteLine(g);
                        }
                        break;

                    case ConsoleKey.F8:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("query:");
                        var query = Console.ReadLine();
                        var graphs = new List<Uri>
                        {
                            new Uri("http://example.org/graph1"),
                            new Uri("http://example.org/graph2")
                        };

                        var resultSet = await triplestoreClient.RunSparqlQuery(dataset, graphs, query);
                        foreach (VDS.RDF.Query.SparqlResult result in resultSet)
                        {
                            Console.WriteLine(result.ToString());
                        }
                        break;
                }

                if (key.Key == ConsoleKey.Escape) break;
            }
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
