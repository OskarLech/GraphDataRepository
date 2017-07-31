using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for ListDatasets.xaml
    /// </summary>
    public partial class ListDatasets : UserControl
    {
        private readonly MainWindow _mainWindow = (MainWindow) Application.Current.MainWindow;

        public ListDatasets()
        {
            InitializeComponent();
        }

        private void ListDatasets_OnLoaded(object sender, EventArgs e)
        {
            GetDatasetList(DataContext);
        }

        public async void GetDatasetList(object dataContext)
        {
            if (DataContext == null)
            {
                DataContext = dataContext;
            }

            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (triplestoreClientQualityWrapper == null)
            {
                _mainWindow.OnOperationFailed();
                return;
            }

            var datasets = await triplestoreClientQualityWrapper.ListDatasets();
            if (datasets != null)
            {
                DatasetListBox.ItemsSource = datasets;
            }
            else
            {
                _mainWindow.OnOperationFailed();
            }
        }

        private async void DatasetListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DatasetListBox.SelectedItems.Count == 0) return;
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (triplestoreClientQualityWrapper == null)
            {
                _mainWindow.OnOperationFailed();
                return;
            }

            var metadataTriples = await triplestoreClientQualityWrapper.GetMetadataTriples(DatasetListBox.SelectedItems[0].ToString());
            if (metadataTriples == null)
            {
                _mainWindow.OnOperationFailed();
                return;
            }

            var text = metadataTriples.Aggregate("", (current, triple) => current + triple.ToString() + "\n");
            ActiveQualityCheckRequirementsTextBox.Text = text;
        }
    }
}
