using System;
using System.Collections.Generic;
using Libraries.Server;
using Libraries.Server.BrightstarDb;
using QualityGrapher.Models;
using QualityGrapher.Utilities.StructureMap;

namespace QualityGrapher.Utilities
{
    public class SupportedTriplestores
    {
        public readonly List<TriplestoreModel> TriplestoreModelList = new List<TriplestoreModel>();
        private static readonly object SyncRoot = new object();
        private static SupportedTriplestores _instance;

        private SupportedTriplestores()
        {
            PopulateModelList();
        }

        public static SupportedTriplestores Instance
        {
            get
            {
                lock (SyncRoot)
                {
                    return _instance ?? (_instance = new SupportedTriplestores());
                }
            }
        }

        private void PopulateModelList()
        {
            var brightstarDb = new TriplestoreModel
            {
                Name = "BrightstarDB",
                SupportedOperations = GetSupportedOperations(typeof(BrightstarClient))
            };

            TriplestoreModelList.Add(brightstarDb);
        }

        private static IEnumerable<string> GetSupportedOperations(Type triplestoreType)
        {
            var supportedOperations = new List<string>();
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

            return supportedOperations;;
        }
    }
}
