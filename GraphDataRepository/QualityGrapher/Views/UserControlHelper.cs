using System.Linq;
using System.Windows;
using Libraries.Server;
using QualityGrapher.ViewModels;

namespace QualityGrapher.Views
{
    public static class UserControlHelper
    {
        public static ITriplestoreClientQualityWrapper GetTriplestoreClientQualityWrapper(object dataContext)
        {
            return ((TriplestoresListViewModel)dataContext)?.SelectedTriplestore?.TriplestoreModel?.TriplestoreClientQualityWrapper;
        }

        public static string GetDatasetFromListDatasetsUserControl(FrameworkElement listDatasetsControl)
        {
            var listDatasetsChildren = LogicalTreeHelper.GetChildren(listDatasetsControl);
            var dataset = listDatasetsChildren.OfType<ListDatasets>()
                .Select(listDatasets => listDatasets.DatasetListBox.SelectedItem?.ToString())
                .FirstOrDefault();

            return dataset;
        }
    }
}
