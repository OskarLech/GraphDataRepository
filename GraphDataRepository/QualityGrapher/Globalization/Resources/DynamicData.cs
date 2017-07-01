using System;
using QualityGrapher.Utilities;

namespace QualityGrapher.Globalization.Resources
{
    public class DynamicData
    {
        public string CurrentLanguage;

        public  string GetTriplestoreOperationText(SupportedOperations operation)
        {
            switch (operation)
            {
                case SupportedOperations.ConsolidateStore:
                    return CurrentLanguage == SupportedLanguages.English ? "Consolidate store" : "Skompresuj bazę serwera";
                case SupportedOperations.CreateDataset:
                    return CurrentLanguage == SupportedLanguages.English ? "Create dataset" : "Stwórz ontologię";
                case SupportedOperations.DeleteDataset:
                    return CurrentLanguage == SupportedLanguages.English ? "Delete dataset" : "Usuń ontologię";
                case SupportedOperations.ListDatasets:
                    return CurrentLanguage == SupportedLanguages.English ? "List dataset" : "Lista ontologii";
                case SupportedOperations.DeleteGraphs:
                    return CurrentLanguage == SupportedLanguages.English ? "Delete graphs" : "Usuń grafy";
                case SupportedOperations.UpdateGraphs:
                    return CurrentLanguage == SupportedLanguages.English ? "Update graphs" : "Zaktualizuj grafy";
                case SupportedOperations.ReadGraphs:
                    return CurrentLanguage == SupportedLanguages.English ? "Read graphs" : "Pobierz grafy";
                case SupportedOperations.ListGraphs:
                    return CurrentLanguage == SupportedLanguages.English ? "List graphs" : "Lista grafów";
                case SupportedOperations.RunSparqlQuery:
                    return CurrentLanguage == SupportedLanguages.English ? "Run SPARQL query" : "Wykonaj zapytanie SPARQL";
                case SupportedOperations.RevertLastTransaction:
                    return CurrentLanguage == SupportedLanguages.English ? "Revert last transaction" : "Cofnij poprzednią operację ";
                case SupportedOperations.ListCommitPoints:
                    return CurrentLanguage == SupportedLanguages.English ? "List commit points" : "Wyświetl punkty kontrolne";
                case SupportedOperations.RevertToCommitPoint:
                    return CurrentLanguage == SupportedLanguages.English ? "Revert to commit point" : "Cofnij do punktu kontrolnego";
                case SupportedOperations.GetStatistics:
                    return CurrentLanguage == SupportedLanguages.English ? "Show statistics" : "Pokaż statystyki";
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }

        public string GetTriplestoreOperation(string operationText)
        {
            throw new NotImplementedException();
        }

        private class TriplestoreOperationTextPl
        {
            public const string ConsolidateStore = "Skompresuj bazę serwera";
        }
    }
}
