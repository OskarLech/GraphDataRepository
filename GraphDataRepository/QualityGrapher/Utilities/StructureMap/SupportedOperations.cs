namespace QualityGrapher.Utilities.StructureMap
{
    public static class SupportedOperations
    {
        //ITriplestoreClient
        public const string CreateDataset = nameof(CreateDataset);
        public const string DeleteDataset = nameof(DeleteDataset);
        public const string ListDatasets = nameof(ListDatasets);
        public const string DeleteGraphs = nameof(DeleteGraphs);
        public const string UpdateGraphs = nameof(UpdateGraphs);
        public const string ReadGraphs = nameof(ReadGraphs);
        public const string ListGraphs = nameof(ListGraphs);
        public const string RunSparqlQuery = nameof(RunSparqlQuery);

        //ITriplestoreClientExtended
        public const string RevertLastTransaction = nameof(RevertLastTransaction);
        public const string ListCommitPoints = nameof(ListCommitPoints);
        public const string RevertToCommitPoint = nameof(RevertToCommitPoint);
        public const string GetStatistics = nameof(GetStatistics);

        //IBrightstarClient
        public const string ConsolidateStore = nameof(ConsolidateStore);
    }
}
