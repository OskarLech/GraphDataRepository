using System.Windows;
using System.Windows.Controls;
using static Serilog.Log;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for ListGraphs.xaml
    /// </summary>
    public partial class ListGraphs : UserControl
    {
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();

        public ListGraphs()
        {
            InitializeComponent();
            ListDatasetControl.Content = _listDatasetsUserControl;
            _listDatasetsUserControl.DatasetListBox.SelectionChanged += OnListDatasetsSelectionChanged;
        }

        private async void OnListDatasetsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataset = ((ListBox)sender).SelectedItem.ToString();
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (triplestoreClientQualityWrapper == null)
            {
                Warning("Could not get triplestore client");
                return;
            }

            var graphList = await triplestoreClientQualityWrapper.ListGraphs(dataset);
            if (graphList == null)
            {
                Warning($"Cannot get the list of graphs from dataset {dataset}");
                return;
            }

            ListGraphsListBox.ItemsSource = graphList;
        }

        private void ListDatasetControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            _listDatasetsUserControl.GetDatasetList(DataContext);
        }
    }
}
