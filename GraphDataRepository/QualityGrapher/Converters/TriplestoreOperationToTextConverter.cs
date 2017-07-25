using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Libraries.Server.SupportedTriplestores;

namespace QualityGrapher.Converters
{
    public class TriplestoreOperationToTextConverter : LanguageConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var operationsToConvert = value as IEnumerable;
            if (operationsToConvert == null) return new List<string>();

            var operationTextList = new List<string>();
            foreach (var operation in operationsToConvert)
            {
                switch (operation)
                {
                    case SupportedOperation.CreateDataset:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.CreateDataset)].ToString());
                        break;
                    case SupportedOperation.DeleteDataset:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.DeleteDataset)].ToString());
                        break;
                    case SupportedOperation.ListDatasets:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.ListDatasets)].ToString());
                        break;
                    case SupportedOperation.CreateGraphs:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.CreateGraphs)].ToString());
                        break;
                    case SupportedOperation.DeleteGraphs:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.DeleteGraphs)].ToString());
                        break;
                    case SupportedOperation.UpdateGraphs:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.UpdateGraphs)].ToString());
                        break;
                    case SupportedOperation.ReadGraphs:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.ReadGraphs)].ToString());
                        break;
                    case SupportedOperation.ListGraphs:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.ListGraphs)].ToString());
                        break;
                    case SupportedOperation.RunSparqlQuery:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.RunSparqlQuery)].ToString());
                        break;
                    case SupportedOperation.RevertLastTransaction:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.RevertLastTransaction)].ToString());
                        break;
                    case SupportedOperation.ListCommitPoints:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.ListCommitPoints)].ToString());
                        break;
                    case SupportedOperation.RevertToCommitPoint:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.RevertToCommitPoint)].ToString());
                        break;
                    case SupportedOperation.GetStatistics:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.GetStatistics)].ToString());
                        break;
                    case SupportedOperation.ConsolidateStore:
                        operationTextList.Add(ResourceDictionary[nameof(SupportedOperation.ConsolidateStore)].ToString());
                        break;
                }
            }

            return operationTextList;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var operation = ResourceDictionary.Keys.Cast<object>().FirstOrDefault(key => ResourceDictionary[key].ToString() == value?.ToString());
            Enum.TryParse<SupportedOperation>(operation?.ToString(), true, out var operationAsEnum);
            return operationAsEnum;
        }
    }
}