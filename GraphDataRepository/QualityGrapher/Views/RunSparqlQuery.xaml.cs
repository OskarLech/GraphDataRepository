using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VDS.RDF.Query;
using static Serilog.Log;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for RunSparqlQuery.xaml
    /// </summary>
    public partial class RunSparqlQuery : UserControl
    {
        private readonly ListGraphs _listGraphsUserControl = new ListGraphs();

        public RunSparqlQuery()
        {
            InitializeComponent();
            ListGraphsControl.Content = _listGraphsUserControl;
        }

        private async void ExecuteQuery_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var dataset = UserControlHelper.GetDatasetFromListDatasetsUserControl(_listGraphsUserControl.ListDatasetsControl);
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (string.IsNullOrWhiteSpace(dataset) || triplestoreClientQualityWrapper == null)
            {
                Warning("Could not get triplestore client");
                return;
            }

            var selectedGraphs = _listGraphsUserControl.ListGraphsListBox.SelectedItems;
            var graphs = (from object graph in selectedGraphs
                          select new Uri(graph.ToString()))
                          .ToList();

            var queryResultSet = await triplestoreClientQualityWrapper.RunSparqlQuery(dataset, graphs, QueryBox.Text);

            if (queryResultSet == null)
            {
                mainWindow.OnOperationFailed();
            }
            else
            {
                var response = queryResultSet.Aggregate<SparqlResult, string>(null, (current, result) => current + result.ToString());
                ResponseBox.Text = response;
                mainWindow.OnOperationSucceeded();
            }
        }
    }
}
