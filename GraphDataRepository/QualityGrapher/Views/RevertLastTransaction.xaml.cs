using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for RevertLastTransaction.xaml
    /// </summary>
    public partial class RevertLastTransaction : UserControl
    {
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();

        public RevertLastTransaction()
        {
            InitializeComponent();
            ListDatasetsControl.Content = _listDatasetsUserControl;
        }

        private async void RevertLastTransactionButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var dataset = _listDatasetsUserControl.DatasetListBox.SelectedItem?.ToString();
            if (triplestoreClientQualityWrapper == null || string.IsNullOrWhiteSpace(dataset) || !await triplestoreClientQualityWrapper.RevertLastTransaction(dataset))
            {
                mainWindow.OnOperationFailed();
            }
            else
            {
                mainWindow.OnOperationSucceeded();
            }
        }
    }
}
