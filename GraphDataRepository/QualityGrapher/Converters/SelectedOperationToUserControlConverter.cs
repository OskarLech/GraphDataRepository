using System;
using System.Globalization;
using System.Windows.Data;
using QualityGrapher.Utilities;
using QualityGrapher.Views;

namespace QualityGrapher.Converters
{
    public class SelectedOperationToUserControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse<SupportedOperations>(value?.ToString(), true, out var operation);
            switch (operation)
            {
                case SupportedOperations.CreateDataset:
                    return new CreateDataset();
                case SupportedOperations.DeleteDataset:
                    return null;
                case SupportedOperations.ListDatasets:
                    return null;
                case SupportedOperations.DeleteGraphs:
                    return null;
                case SupportedOperations.UpdateGraphs:
                    return null;
                case SupportedOperations.ReadGraphs:
                    return null;
                case SupportedOperations.ListGraphs:
                    return null;
                case SupportedOperations.RunSparqlQuery:
                    return null;
                case SupportedOperations.RevertLastTransaction:
                    return null;
                case SupportedOperations.ListCommitPoints:
                    return null;
                case SupportedOperations.RevertToCommitPoint:
                    return null;
                case SupportedOperations.GetStatistics:
                    return null;
                case SupportedOperations.ConsolidateStore:
                    return null;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
