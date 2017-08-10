using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VDS.RDF;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for ReadGraphs.xaml
    /// </summary>
    public partial class ReadGraphs : UserControl
    {
        private readonly ListGraphs _listGraphsUserControl = new ListGraphs();

        public ReadGraphs()
        {
            InitializeComponent();
            ListGraphsControl.Content = _listGraphsUserControl;
            _listGraphsUserControl.ListGraphsListBox.SelectionMode = SelectionMode.Single;
        }

        private async void ReadGraphsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var dataset = UserControlHelper.GetDatasetFromListDatasetsUserControl(_listGraphsUserControl.ListDatasetsControl);
            if (triplestoreClientQualityWrapper == null || string.IsNullOrWhiteSpace(dataset))
            {
                mainWindow.OnOperationFailed();
                return;
            }

            var graphUri = _listGraphsUserControl.ListGraphsListBox.SelectedItem as Uri;
            if (graphUri == null)
            {
                mainWindow.OnOperationFailed();
                return;
            }

            var graph = await triplestoreClientQualityWrapper.ReadGraphs(dataset, graphUri.AsEnumerable());
            if (graph == null)
            {
                mainWindow.OnOperationFailed();
                return;
            }

            var triplesList = graph.FirstOrDefault()?.Triples.ToList();
            GraphTriples.Text = triplesList?.Aggregate<Triple, string>(null, (current, triple) => current + triple.ToString() + "\n");
            mainWindow.OnOperationSucceeded();
        }
    }
}
