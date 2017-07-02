using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using QualityGrapher.Globalization;
using QualityGrapher.Views;
using static Libraries.Server.SupportedTriplestores;

namespace QualityGrapher.Converters
{
    public class TriplestoreOperationToTextConverter : IValueConverter
    {
        private ResourceDictionary _resourceDictionary;

        public TriplestoreOperationToTextConverter()
        {
            SetResourceDictionary(Thread.CurrentThread.CurrentCulture.ToString());
            ((MainWindow)Application.Current.MainWindow).LanguageSet += OnLanguageSet;
        }

        private void OnLanguageSet(string language)
        {
            SetResourceDictionary(language);
        }

        private void SetResourceDictionary(string language)
        {
            _resourceDictionary = new ResourceDictionary
            {
                Source = language == SupportedLanguages.Polish
                    ? new Uri(@"..\Globalization\Resources\pl-PL.xaml", UriKind.RelativeOrAbsolute)
                    : new Uri(@"..\Globalization\Resources\en-GB.xaml", UriKind.RelativeOrAbsolute)
            };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var operationsToConvert = value as IEnumerable;
            if (operationsToConvert == null) return new List<string>();

            var operationTextList = new List<string>();
            foreach (var operation in operationsToConvert)
            {
                switch (operation)
                {
                    case SupportedOperations.CreateDataset:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.CreateDataset)].ToString());
                        break;
                    case SupportedOperations.DeleteDataset:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.DeleteDataset)].ToString());
                        break;
                    case SupportedOperations.ListDatasets:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.ListDatasets)].ToString());
                        break;
                    case SupportedOperations.DeleteGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.DeleteGraphs)].ToString());
                        break;
                    case SupportedOperations.UpdateGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.UpdateGraphs)].ToString());
                        break;
                    case SupportedOperations.ReadGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.ReadGraphs)].ToString());
                        break;
                    case SupportedOperations.ListGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.ListGraphs)].ToString());
                        break;
                    case SupportedOperations.RunSparqlQuery:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.RunSparqlQuery)].ToString());
                        break;
                    case SupportedOperations.RevertLastTransaction:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.RevertLastTransaction)].ToString());
                        break;
                    case SupportedOperations.ListCommitPoints:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.ListCommitPoints)].ToString());
                        break;
                    case SupportedOperations.RevertToCommitPoint:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.RevertToCommitPoint)].ToString());
                        break;
                    case SupportedOperations.GetStatistics:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.GetStatistics)].ToString());
                        break;
                    case SupportedOperations.ConsolidateStore:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperations.ConsolidateStore)].ToString());
                        break;
                }
            }

            return operationTextList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var operation = _resourceDictionary.Keys.Cast<object>().FirstOrDefault(key => _resourceDictionary[key].ToString() == value?.ToString());
            Enum.TryParse<SupportedOperations>(operation?.ToString(), true, out var operationAsEnum);
            return operationAsEnum;
        }
    }
}