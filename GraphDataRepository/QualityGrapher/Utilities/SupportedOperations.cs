namespace QualityGrapher.Utilities
{
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
}
