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
        private readonly ListCommitPoints _listCommitPointsUserControl = new ListCommitPoints();

        public RevertToCommitPoint()
        {
            InitializeComponent();
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
            var dataset = UserControlHelper.GetDatasetFromListDatasetsUserControl(_listCommitPointsUserControl.ListDatasetsControl);

            var commitPoint = _listCommitPointsUserControl.CommitInfoList.SelectedCommit;
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
