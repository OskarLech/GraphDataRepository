using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for DeleteGraphs.xaml
    /// </summary>
    public partial class DeleteGraphs : UserControl
    {
        private readonly ListGraphs _listGraphsUserControl = new ListGraphs();

        public DeleteGraphs()
        {
            InitializeComponent();
            ListGraphsControl.Content = _listGraphsUserControl;
        }

        private async void DeleteGraphButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var listDatasetsChildren = LogicalTreeHelper.GetChildren(_listGraphsUserControl.ListDatasetsControl);
            var triplestore = listDatasetsChildren.OfType<ListDatasets>()
                .Select(listDatasets => listDatasets.DatasetListBox.SelectedItem?.ToString())
                .FirstOrDefault();

            var graphs = _listGraphsUserControl.ListGraphsListBox.SelectedItems.Cast<Uri>();
            if (triplestoreClientQualityWrapper == null || string.IsNullOrWhiteSpace(triplestore) || !await triplestoreClientQualityWrapper.DeleteGraphs(triplestore, graphs))
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
