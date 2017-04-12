using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using BrightstarDB;
using BrightstarDB.Client;
using BrightstarDB.EntityFramework;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using Vocab;
using SparqlResult = BrightstarDB.Client.SparqlResult;

namespace GraphDataRepository
{
    class Program
    {
        static void Main(string[] args)
        {

            TestGraph();

            /********************************/
            /***********BRIGHTSTAR**********/
            /********************************/

            var client = BrightstarService.GetClient("Type=rest;endpoint=http://localhost:8090/brightstar;");
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

            SparqlConnector connect = new SparqlConnector(new Uri($"http://localhost:8090/brightstar/{storeName}//SPARQL"));
            PersistentTripleStore store = new PersistentTripleStore(connect);

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

        private void Test()
        {
            //var rdfsLabel = Foaf.geekcode;
            //var g = new Graph();

            //var context = BrightstarService.GetDataObjectContext("Type=rest;endpoint=http://localhost:8090/brightstar;");
            //{
            //    context.CreateStore("MyStore");
            //}

            //if (context.DoesStoreExist("MyStore"))
            //{
            //    return;
            //}

            //var store = context.OpenStore("MyStore");
            //var fred = store.MakeDataObject("http://example.org/people/fred");
            //var name = store.MakeDataObject("http://xmlns.com/foaf/0.1/name");
            //fred.SetProperty(name, "Fred Evans");

            //var x = fred.GetPropertyTypes();

            //Console.WriteLine("Properties of Fred:");
            //foreach (var propertyType in fred.GetPropertyTypes())
            //{
            //    Console.WriteLine("\t" + propertyType.Identity + ":");
            //    foreach (var propertyValue in fred.GetPropertyValues(propertyType))
            //    {
            //        Console.WriteLine("\t\t" + propertyValue);
            //    }
            //}

            //Console.ReadLine();

            // define a connection string
            const string connectionString = "type=embedded;storesdirectory=.\\;storename=Films";
            // if the store does not exist it will be automatically
            // created when a context is created
            var ctx = new MyEntityContext(connectionString);
            // create some films

            var bladeRunner = ctx.Films.Create();
            bladeRunner.Name = "BladeRunner";
            var starWars = ctx.Films.Create();
            starWars.Name = "Star Wars";
            // create some actors and connect them to films
            var ford = ctx.Actors.Create();
            ford.Name = "Harrison Ford";
            ford.DateOfBirth = new DateTime(1942, 7, 13);
            ford.Films.Add(starWars);
            ford.Films.Add(bladeRunner);
            var hamill = ctx.Actors.Create();
            hamill.Name = "Mark Hamill";
            hamill.DateOfBirth = new DateTime(1951, 9, 25);
            hamill.Films.Add(starWars);
            // save the data
            ctx.SaveChanges();
            // open a new context, not required
            ctx = new MyEntityContext(connectionString);
            // find an actor via LINQ
            ford = ctx.Actors.FirstOrDefault(a => a.Name.Equals("Harrison Ford"));
            var dob = ford.DateOfBirth;
            // list his films
            var films = ford.Films;
            // get star wars
            var sw = films.FirstOrDefault(f => f.Name.Equals("Star Wars"));
            // list actors in star wars
            foreach (var actor in sw.Actors)
            {
                var actorName = actor.Name;
                Console.WriteLine(actorName);
            }
            Console.ReadLine();
        }
    }

    [Entity]
    public interface IActor
    {
        string Name { get; set; }
        DateTime DateOfBirth { get; set; }
        ICollection<IFilm> Films { get; set; }
    }

    [Entity]
    public interface IFilm
    {
        string Name { get; set; }
        [InverseProperty("Films")]
        ICollection<IActor> Actors { get; }
    }
}
