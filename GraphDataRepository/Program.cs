using System;
using System.Threading;
using BrightstarDB.Client;
using VDS.RDF;
using Vocab;

namespace GraphDataRepository
{
    class Program
    {
        static void Main(string[] args)
        {
            var rdfsLabel = Foaf.geekcode;
            var g = new Graph();

            var context = BrightstarService.GetDataObjectContext("Type=rest;endpoint=http://localhost:8090/brightstar;");
            {
                context.CreateStore("MyStore");
            }

            if (context.DoesStoreExist("MyStore"))
            {
                return;
            }

            var store = context.OpenStore("MyStore");
            var fred = store.MakeDataObject("http://example.org/people/fred");            var name = store.MakeDataObject("http://xmlns.com/foaf/0.1/name");
            fred.SetProperty(name, "Fred Evans");

            var x = fred.GetPropertyTypes();

            Console.WriteLine("Properties of Fred:");
            foreach (var propertyType in fred.GetPropertyTypes())
            {
                Console.WriteLine("\t" + propertyType.Identity + ":");
                foreach (var propertyValue in fred.GetPropertyValues(propertyType))
                {
                    Console.WriteLine("\t\t" + propertyValue);
                }
            }

            Console.ReadLine();
        }
    }
}
