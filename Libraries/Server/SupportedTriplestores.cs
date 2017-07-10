using System;
using System.Collections.Generic;
using System.Linq;
using Libraries.Server.BrightstarDb;

namespace Libraries.Server
{
    public static class SupportedTriplestores
    {
        public static IEnumerable<(string name, Type clientType)> TriplestoreProviders = new List<(string Name, Type Type)>
        {
            ("BrightstarDb", typeof(BrightstarClient))
        };

        public enum SupportedOperation
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

        public static IEnumerable<(string Name, Type Type, IEnumerable<SupportedOperation> SupportedOperations)> GetTriplestoresList()
        {
            return TriplestoreProviders.Select(triplestoreProvider => (triplestoreProvider.name, triplestoreProvider.clientType, GetSupportedOperations(triplestoreProvider.clientType)))
                .ToList();
        }

        public static IEnumerable<SupportedOperation> GetSupportedOperations(Type triplestoreType)
        {
            var supportedOperations = new List<SupportedOperation>();
            if (typeof(ITriplestoreClient).IsAssignableFrom(triplestoreType))
            {
                supportedOperations.Add(SupportedOperation.CreateDataset);
                supportedOperations.Add(SupportedOperation.DeleteDataset);
                supportedOperations.Add(SupportedOperation.ListDatasets);
                supportedOperations.Add(SupportedOperation.DeleteGraphs);
                supportedOperations.Add(SupportedOperation.UpdateGraphs);
                supportedOperations.Add(SupportedOperation.ReadGraphs);
                supportedOperations.Add(SupportedOperation.ListGraphs);
                supportedOperations.Add(SupportedOperation.RunSparqlQuery);
            }

            if (typeof(ITriplestoreClientExtended).IsAssignableFrom(triplestoreType))
            {
                supportedOperations.Add(SupportedOperation.RevertLastTransaction);
                supportedOperations.Add(SupportedOperation.ListCommitPoints);
                supportedOperations.Add(SupportedOperation.RevertToCommitPoint);
                supportedOperations.Add(SupportedOperation.GetStatistics);
            }

            if (typeof(IBrightstarClient).IsAssignableFrom(triplestoreType))
            {
                supportedOperations.Add(SupportedOperation.ConsolidateStore);
            }

            return supportedOperations;
        }
    }

}
