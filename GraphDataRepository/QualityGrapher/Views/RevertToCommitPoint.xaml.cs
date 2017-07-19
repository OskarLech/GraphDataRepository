using System;
using System.Windows;
using System.Windows.Controls;
using static Serilog.Log;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for RevertToCommitPoint.xaml
    /// </summary>
    public partial class RevertToCommitPoint : UserControl
    {
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();
        private readonly ListCommitPoints _listCommitPointsUserControl = new ListCommitPoints();

        public RevertToCommitPoint()
        {
            InitializeComponent();
            ListDatasetsControl.Content = _listDatasetsUserControl;
            ListCommitPointsControl.Content = _listCommitPointsUserControl;
        }

        private async void RevertButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (triplestoreClientQualityWrapper == null)
            {
                return;
            }

            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var dataset = _listDatasetsUserControl.DatasetListBox.SelectedItem.ToString();
            var commitPoint = ((ulong Id, DateTime commitDate)) _listCommitPointsUserControl.CommitPointsListBox.SelectedItem;
            if (await triplestoreClientQualityWrapper.RevertToCommitPoint(dataset, commitPoint.Id))
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
