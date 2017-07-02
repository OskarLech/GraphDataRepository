using System;
using System.Collections.Generic;
using System.Linq;
using Libraries.Server.BrightstarDb;

namespace Libraries.Server
{
    public static class SupportedTriplestores
    {
        public static IEnumerable<(string name, Type clientClass)> TriplestoreProviders = new List<(string name, Type type)>
        {
            ("BrightstarDb", typeof(BrightstarClient))
        };

        public enum SupportedOperations
        {
            //ITriplestoreClient
            CreateDataset,
            DeleteDataset,
            ListDatasets,
            DeleteGraphs,
            UpdateGraphs,
            ReadGraphs,
            ListGraphs,
            RunSparqlQuery,

            //ITriplestoreClientExtended
            RevertLastTransaction,
            ListCommitPoints,
            RevertToCommitPoint,
            GetStatistics,

            //IBrightstarClient
            ConsolidateStore
        }

        public static IEnumerable<(string name, IEnumerable<SupportedOperations> supportedOperations)> GetTriplestoresList()
        {
            return TriplestoreProviders.Select(triplestoreProvider => (triplestoreProvider.name, GetSupportedOperations(triplestoreProvider.clientClass))).ToList();
        }

        public static IEnumerable<SupportedOperations> GetSupportedOperations(Type triplestoreType)
        {
            var supportedOperations = new List<SupportedOperations>();
            if (typeof(ITriplestoreClient).IsAssignableFrom(triplestoreType))
            {
                supportedOperations.Add(SupportedOperations.CreateDataset);
                supportedOperations.Add(SupportedOperations.DeleteDataset);
                supportedOperations.Add(SupportedOperations.ListDatasets);
                supportedOperations.Add(SupportedOperations.DeleteGraphs);
                supportedOperations.Add(SupportedOperations.UpdateGraphs);
                supportedOperations.Add(SupportedOperations.ReadGraphs);
                supportedOperations.Add(SupportedOperations.ListGraphs);
                supportedOperations.Add(SupportedOperations.RunSparqlQuery);
            }

            if (typeof(ITriplestoreClientExtended).IsAssignableFrom(triplestoreType))
            {
                supportedOperations.Add(SupportedOperations.RevertLastTransaction);
                supportedOperations.Add(SupportedOperations.ListCommitPoints);
                supportedOperations.Add(SupportedOperations.RevertToCommitPoint);
                supportedOperations.Add(SupportedOperations.GetStatistics);
            }

            if (typeof(IBrightstarClient).IsAssignableFrom(triplestoreType))
            {
                supportedOperations.Add(SupportedOperations.ConsolidateStore);
            }

            return supportedOperations;
        }
    }

}
