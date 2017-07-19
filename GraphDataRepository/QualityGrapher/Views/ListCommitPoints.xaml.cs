using System.Windows;
using System.Windows.Controls;
using QualityGrapher.ViewModels;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for ListCommitPoints.xaml
    /// </summary>
    public partial class ListCommitPoints : UserControl
    {
        public CommitPointListViewModel CommitInfoList = new CommitPointListViewModel();
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();

        public ListCommitPoints()
        {
            InitializeComponent();
            ListDatasetsControl.Content = _listDatasetsUserControl;
            CommitPointsListBox.DataContext = CommitInfoList;
        }

        private async void ListCommitPointsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (triplestoreClientQualityWrapper == null)
            {
                mainWindow.OnOperationFailed();
                return;
            }

            var dataset = _listDatasetsUserControl.DatasetListBox.SelectedItem.ToString();
            CommitInfoList.CommitInfoList = await triplestoreClientQualityWrapper.ListCommitPoints(dataset);
            if (string.IsNullOrWhiteSpace(dataset) || CommitInfoList == null)
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
