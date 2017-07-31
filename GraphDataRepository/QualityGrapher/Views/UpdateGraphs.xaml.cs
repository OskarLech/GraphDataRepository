using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using QualityGrapher.ViewModels;
using static Libraries.QualityChecks.QualityChecksData;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for UpdateGraphs.xaml
    /// </summary>
    public partial class UpdateGraphs : UserControl
    {
        private readonly MainWindow _mainWindow = (MainWindow)Application.Current.MainWindow;
        private readonly ListGraphs _listGraphsUserControl = new ListGraphs();

        public UpdateGraphs()
        {
            InitializeComponent();
            ListGraphsControl.Content = _listGraphsUserControl;
        }

        /// <summary>
        /// Selecting metadata graph adds quality check requirement for whole dataset. User can either add quality check for 
        /// whole dataset or for each graph specifically
        /// </summary>
        /// <param name="graphs">Selected graphs</param>
        /// <returns>Flag indicating if graph selection is correct</returns>
        private static bool IsGraphSelectionCorrect(IEnumerable<Uri> graphs)
        {
            var graphList = graphs as IList<Uri> ?? graphs.ToList();
            return graphList.Any() && (graphList.Count <= 1 || graphList.All(g => g != MetadataGraphUri)); 
        }

        private async void UpdateGraphsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);

            var dataset = UserControlHelper.GetDatasetFromListDatasetsUserControl(_listGraphsUserControl.ListDatasetsControl);
            var graphs = _listGraphsUserControl.ListGraphsListBox.SelectedItems.Cast<Uri>();

            IEnumerable<string> triplesToRemove = TriplesToRemoveTextbox.Text.Split("\n".ToCharArray()).ToList();
            IEnumerable<string> triplesToAdd = TriplesToAddTextbox.Text.Split("\n".ToCharArray()).ToList();
            var triplesByGraphUri = graphs.ToDictionary(graph => graph, graph => (triplesToAdd, triplesToRemove));

            if(triplestoreClientQualityWrapper == null || string.IsNullOrWhiteSpace(dataset) || !await triplestoreClientQualityWrapper.UpdateGraphs(dataset, triplesByGraphUri))
            {
                _mainWindow.OnOperationFailed();
            }
            else
            {
                _mainWindow.OnOperationSucceeded();
            }
        }

        private void QualityCheckListBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            QualityCheckComboBox.DataContext = new QualityCheckListViewModel();
            QualityCheckComboBox.SelectedIndex = 0;
        }

        private void AddQualityCheckButton_OnClick(object sender, RoutedEventArgs e)
        {
            var graphs = _listGraphsUserControl.ListGraphsListBox.SelectedItems.Cast<Uri>()
                .ToList();

            if (!IsGraphSelectionCorrect(graphs))
            {
                _mainWindow.OnOperationFailed();
                return;
            }

            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (graphs.Any(g => g == MetadataGraphUri))
            {
                throw new System.NotImplementedException();
            }

            throw new System.NotImplementedException();
        }

        private void RemoveQualityCheckButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
