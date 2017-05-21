using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using BrightstarDB;
using BrightstarDB.Client;
using GraphDataRepository.Server.BrightstarDb;
using GraphDataRepository.Utilities.StructureMap;
using log4net;
using log4net.Config;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace GraphDataRepository
{
    internal class Program
    {
        private static ILog _log;
        private static readonly List<IDisposable> Disposables = new List<IDisposable>();

        static void Main(string[] args)
        {
            Initialize();

            /*************/
            BrightstarClientCli();
            /*************/

            var mre = new ManualResetEvent(false);
            mre.WaitOne();

            Console.WriteLine("Press Enter to stop the program");
            Console.ReadLine();

            _log.Info("Terminating the program");
            Disposables.ForEach(d => d.Dispose());
            _log.Info("Program terminated succesfully");
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
                        if (await triplestoreClient.DeleteGraph(dataset, graph))
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

                        if (await triplestoreClient.UpdateGraph(dataset, graph, triplesToRemove, triplesToAdd))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.F6:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("graph URI:");
                        graph = Console.ReadLine();

                        var dnrGraph = await triplestoreClient.ReadGraph(dataset, graph);
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
                        var resultSet = await triplestoreClient.RunSparqlQuery(dataset, query);
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
            //log4net
            XmlConfigurator.Configure();
            _log = ObjectFactory.Container.GetInstance<ILog>();
        }

        private static void Playground()
        {
            //TestGraph();

            /********************************/
            /***********BRIGHTSTAR**********/
            /********************************/
            
            var client = BrightstarService.GetClient("Type=rest;endpoint=http://192.168.0.111:8090/brightstar;");
            string storeName = "Store_" + Guid.NewGuid();
            client.CreateStore(storeName);

            var addTriples = new StringBuilder();
            addTriples.AppendLine(
                "<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/name> \"BrightstarDB\" <http://example.org/graphs/alice> .");
            addTriples.AppendLine(
                "<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/category> <http://www.brightstardb.com/categories/nosql> <http://example.org/graphs/alice> .");
            addTriples.AppendLine(
                "<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/category> <http://www.brightstardb.com/categories/.net> <http://example.org/graphs/alice> .");
            addTriples.AppendLine(
                "<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/category> <http://www.brightstardb.com/categories/rdf> <http://example.org/graphs/alice> .");
            var transactionData = new UpdateTransactionData
            {
                InsertData = addTriples.ToString(),
                //DefaultGraphUri = "http://example.org/graphs/alice"
            };

            var jobInfo = client.ExecuteTransaction(storeName, transactionData);
            var namedGraphs = client.ListNamedGraphs(storeName);

            var query = "SELECT ?category WHERE { " +
                        "<http://www.brightstardb.com/products/brightstar> <http://www.brightstardb.com/schemas/product/category> ?category ." +
                        "}";

            var result = XDocument.Load(client.ExecuteQuery(storeName, query, "http://example.org/graphs/alice"));
            var result2 = client.ExecuteQuery(storeName, query, resultsFormat: SparqlResultsFormat.Xml);

            /********************************/
            /************DOTNETRDF***********/
            /********************************/

            SparqlConnector connect = new SparqlConnector(new Uri($"http://192.168.0.111:8090/brightstar/{storeName}//SPARQL"));
            PersistentTripleStore store = new PersistentTripleStore(connect);
            var aliceGraph = new Graph();
            connect.LoadGraph(aliceGraph, "http://example.org/graphs/alice");

            Object results = store.ExecuteQuery(query);//TODO: search in !default graph
            if (results is SparqlResultSet)
            {
                //Print out the results
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (VDS.RDF.Query.SparqlResult resultxxxx in rset)
                {
                    Console.WriteLine(resultxxxx.ToString());
                }

            }

            var y = store.UnderlyingStore.ListGraphs();

            foreach (var graphUri in store.UnderlyingStore.ListGraphs())
            {
                var z = store.Graphs.FirstOrDefault(g => g.BaseUri == graphUri);
            }

            //IGraph g = new Graph();
            //var x = new RdfXmlParser();
            //x.Load(g, result);

            //var gNamespaceMap = g.NamespaceMap;
            //foreach (var triple in g.Triples)
            //{
            //    Console.WriteLine(triple.Predicate);
            //}
            Console.WriteLine("KUNIEC");
            Console.ReadLine();
        }

        private static void TestGraph()
        {
            var dataGraph = new Graph();
            FileLoader.Load(dataGraph, @"..\..\TestData\RDF\foaf_example.rdf");

            var schemaGraph = new Graph();
            FileLoader.Load(schemaGraph, @"..\..\TestData\Schemas\foaf_20140114.rdf");

            var subjectList = schemaGraph.Triples.Select(triple => triple.Subject.ToString()).Distinct().ToList();

            var wrongPredicates = new List<string>();
            foreach (var triple in dataGraph.Triples)
            {
                var predicate = triple.Predicate.ToString();
                if (!subjectList.Contains(predicate) && !wrongPredicates.Contains(predicate))
                {
                    wrongPredicates.Add(predicate);
                }
            }

            foreach (var predicate in wrongPredicates)
            {
                Console.WriteLine(predicate);
            }
        }
    }
}
