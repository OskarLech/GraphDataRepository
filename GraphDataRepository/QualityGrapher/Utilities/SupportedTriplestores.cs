using System;
using System.Collections.Generic;
using Libraries.Server;
using Libraries.Server.BrightstarDb;
using QualityGrapher.Globalization.Resources;
using QualityGrapher.Utilities.StructureMap;
using QualityGrapher.ViewModels;

namespace QualityGrapher.Utilities
{
    public class SupportedTriplestores
    {
        private static readonly object SyncRoot = new object();
        private static SupportedTriplestores _instance;
        public readonly List<TriplestoreViewModel> TriplestoreModelList = new List<TriplestoreViewModel>();
        private readonly DynamicData _dynamicData = ObjectFactory.Container.GetInstance<DynamicData>();

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
        
        public enum TriplestoreProviders
        {
            BrightstarDb
        }

        private void PopulateModelList()
        {
            var brightstarDb = new TriplestoreViewModel
            {
                Name = TriplestoreProviders.BrightstarDb.ToString(),
                SupportedOperations = GetSupportedOperations(typeof(BrightstarClient))
            };

            TriplestoreModelList.Add(brightstarDb);
        }

        private Dictionary<SupportedOperations, string> GetSupportedOperations(Type triplestoreType)
        {
            var supportedOperations = new Dictionary<SupportedOperations, string>();
            if (typeof(ITriplestoreClient).IsAssignableFrom(triplestoreType))
            {
                supportedOperations[SupportedOperations.CreateDataset] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.CreateDataset);
                supportedOperations[SupportedOperations.DeleteDataset] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.DeleteDataset);
                supportedOperations[SupportedOperations.ListDatasets] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.ListDatasets);
                supportedOperations[SupportedOperations.DeleteGraphs] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.DeleteGraphs);
                supportedOperations[SupportedOperations.UpdateGraphs] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.UpdateGraphs);
                supportedOperations[SupportedOperations.ReadGraphs] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.ReadGraphs);
                supportedOperations[SupportedOperations.ListGraphs] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.ListGraphs);
                supportedOperations[SupportedOperations.RunSparqlQuery] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.RunSparqlQuery);
            }                      

            if (typeof(ITriplestoreClientExtended).IsAssignableFrom(triplestoreType))
            {
                supportedOperations[SupportedOperations.RevertLastTransaction] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.RevertLastTransaction);
                supportedOperations[SupportedOperations.ListCommitPoints] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.ListCommitPoints);
                supportedOperations[SupportedOperations.RevertToCommitPoint] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.RevertToCommitPoint);
                supportedOperations[SupportedOperations.GetStatistics] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.GetStatistics);
            }

            if (typeof(IBrightstarClient).IsAssignableFrom(triplestoreType))
            {
                supportedOperations[SupportedOperations.ConsolidateStore] = _dynamicData.GetTriplestoreOperationText(SupportedOperations.ConsolidateStore);
            }

            return supportedOperations;
        }
    }
}
