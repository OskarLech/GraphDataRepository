using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for GetStatistics.xaml
    /// </summary>
    public partial class GetStatistics : UserControl
    {
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();

        public GetStatistics()
        {
            InitializeComponent();
            ListDatasetsControl.Content = _listDatasetsUserControl;
        }

        private async void GetStatisticsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (triplestoreClientQualityWrapper == null)
            {
                mainWindow.OnOperationFailed();
                return;
            }

            var dataset = _listDatasetsUserControl.DatasetListBox.SelectedItem.ToString();
            var statistics = await triplestoreClientQualityWrapper.GetStatistics(dataset);
            if (string.IsNullOrWhiteSpace(dataset) || string.IsNullOrWhiteSpace(statistics))
            {
                mainWindow.OnOperationFailed();
            }
            else
            {
                StatisticsTextBox.Text = statistics;
                mainWindow.OnOperationSucceeded();
            }
        }
    }
}
