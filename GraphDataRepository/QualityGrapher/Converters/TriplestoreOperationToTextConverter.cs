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
                    case SupportedOperation.CreateDataset:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.CreateDataset)].ToString());
                        break;
                    case SupportedOperation.DeleteDataset:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.DeleteDataset)].ToString());
                        break;
                    case SupportedOperation.ListDatasets:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.ListDatasets)].ToString());
                        break;
                    case SupportedOperation.DeleteGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.DeleteGraphs)].ToString());
                        break;
                    case SupportedOperation.UpdateGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.UpdateGraphs)].ToString());
                        break;
                    case SupportedOperation.ReadGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.ReadGraphs)].ToString());
                        break;
                    case SupportedOperation.ListGraphs:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.ListGraphs)].ToString());
                        break;
                    case SupportedOperation.RunSparqlQuery:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.RunSparqlQuery)].ToString());
                        break;
                    case SupportedOperation.RevertLastTransaction:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.RevertLastTransaction)].ToString());
                        break;
                    case SupportedOperation.ListCommitPoints:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.ListCommitPoints)].ToString());
                        break;
                    case SupportedOperation.RevertToCommitPoint:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.RevertToCommitPoint)].ToString());
                        break;
                    case SupportedOperation.GetStatistics:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.GetStatistics)].ToString());
                        break;
                    case SupportedOperation.ConsolidateStore:
                        operationTextList.Add(_resourceDictionary[nameof(SupportedOperation.ConsolidateStore)].ToString());
                        break;
                }
            }

            return operationTextList;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var operation = _resourceDictionary.Keys.Cast<object>().FirstOrDefault(key => _resourceDictionary[key].ToString() == value?.ToString());
            Enum.TryParse<SupportedOperation>(operation?.ToString(), true, out var operationAsEnum);
            return operationAsEnum;
        }
    }
}