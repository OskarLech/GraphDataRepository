using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for ConsolidateStore.xaml
    /// </summary>
    public partial class ConsolidateStore : UserControl
    {
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();

        public ConsolidateStore()
        {
            InitializeComponent();
            ListDatasetsControl.Content = _listDatasetsUserControl;
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow) Application.Current.MainWindow;

            var dataset = _listDatasetsUserControl.DatasetListBox.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(dataset) || triplestoreClientQualityWrapper == null || 
                string.IsNullOrWhiteSpace(dataset) || !await triplestoreClientQualityWrapper.ConsolidateDataset(dataset))
            {
                mainWindow.OnOperationFailed();
            }
            else
            {
                mainWindow.OnOperationSucceeded();
            }
        }

        private void ListDatasetControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            _listDatasetsUserControl.GetDatasetList(DataContext);
        }
    }
}
