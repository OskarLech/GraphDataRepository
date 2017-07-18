using System;
using System.Globalization;
using System.Windows.Data;
using QualityGrapher.Views;
using static Libraries.Server.SupportedTriplestores;

namespace QualityGrapher.Converters
{
    public class SelectedOperationToUserControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse<SupportedOperation>(value?.ToString(), true, out var operation);
            switch (operation)
            {
                case SupportedOperation.CreateDataset:
                    return new CreateDataset();
                case SupportedOperation.DeleteDataset:
                    return new DeleteDataset();
                case SupportedOperation.ListDatasets:
                    return new ListDatasets();
                case SupportedOperation.DeleteGraphs:
                    return new DeleteGraphs();
                case SupportedOperation.UpdateGraphs:
                    return new UpdateGraphs();
                case SupportedOperation.ReadGraphs:
                    return new ReadGraphs();
                case SupportedOperation.ListGraphs:
                    return new ListGraphs();
                case SupportedOperation.RunSparqlQuery:
                    //return new RunSparqlQuery();
                case SupportedOperation.RevertLastTransaction:
                    //return new RevertLastTransaction();
                case SupportedOperation.ListCommitPoints:
                    //return new ListCommitPoints();
                case SupportedOperation.RevertToCommitPoint:
                    //return new RevertToCommitPoint();
                case SupportedOperation.GetStatistics:
                    //return new GetStatistics();
                case SupportedOperation.ConsolidateStore:
                    //return new ConsolidateStore();
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
