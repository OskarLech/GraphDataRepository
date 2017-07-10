using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for DeleteDataset.xaml
    /// </summary>
    public partial class DeleteDataset : UserControl
    {
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();
        public DeleteDataset()
        {
            InitializeComponent();
            ListDatasetControl.Content = _listDatasetsUserControl;
            _listDatasetsUserControl.DatasetListBox.SelectionChanged += (sender, args) =>
                DeleteDatasetTextBox.Text = _listDatasetsUserControl.DatasetListBox.SelectedItem?.ToString();
        }

        private async void DeleteDatasetButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (triplestoreClientQualityWrapper == null || !await triplestoreClientQualityWrapper.DeleteDataset(DeleteDatasetTextBox.Text))
            {
                mainWindow.OnOperationFailed();
            }
            else
            {
                mainWindow.OnOperationSucceeded();
                _listDatasetsUserControl.GetDatasetList(DataContext);
            }
        }

        private void ListDatasetControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            _listDatasetsUserControl.GetDatasetList(DataContext);
        }
    }
}
