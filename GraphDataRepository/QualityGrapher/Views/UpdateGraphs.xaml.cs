using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            IList<string> triplesToRemove = TriplesToRemoveTextbox.Text.Split("\n".ToCharArray()).ToList();
            IList<string> triplesToAdd = TriplesToAddTextbox.Text.Split("\n".ToCharArray()).ToList();
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

        private async void AddQualityCheckButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await ModifyQualityChecks(OperationToPerform.AddQualityChecks))
            {
                _mainWindow.OnOperationSucceeded();
                return;
            }

            _mainWindow.OnOperationFailed();
        }

        private async void RemoveQualityCheckButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (await ModifyQualityChecks(OperationToPerform.RemoveQualityChecks))
            {
                _mainWindow.OnOperationSucceeded();
                return;
            }

            _mainWindow.OnOperationFailed();
        }

        private async Task<bool> ModifyQualityChecks(OperationToPerform operationToPerform)
        {
            var graphs = _listGraphsUserControl.ListGraphsListBox.SelectedItems.Cast<Uri>()
                .ToList();

            if (!IsGraphSelectionCorrect(graphs))
            {
                return false;
            }

            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var dataset = UserControlHelper.GetDatasetFromListDatasetsUserControl(_listGraphsUserControl.ListDatasetsControl);

            var qualityCheckName = ((QualityCheckListViewModel)QualityCheckComboBox.DataContext).SelectedQualityCheck.ToString();
            var qualityCheckPredicate = QualityCheckInstances.FirstOrDefault(i => i.GetPredicate().Contains(qualityCheckName))?.GetPredicate();
            if (qualityCheckPredicate == null)
            {
                return false;
            }

            var parameters = QualityCheckParameterTextBox.Text.Split(new[] {"\n"}, StringSplitOptions.None);
            var triplesByGraphUri = new Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)>();
            if (graphs.Any(g => g == MetadataGraphUri))
            {
                var triplesToModify = parameters.Select(parameter => $"{WholeDatasetSubjectUri} , {qualityCheckPredicate} , {parameter}").ToList();
                if (operationToPerform == OperationToPerform.AddQualityChecks)
                {
                    triplesByGraphUri = new Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)>
                    {
                        [MetadataGraphUri] = (new List<string>() , triplesToModify)
                    };
                }
                else
                {
                    triplesByGraphUri = new Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)>
                    {
                        [MetadataGraphUri] = (triplesToModify, new List<string>())
                    };
                }
            }
            else
            {
                if (!triplesByGraphUri.ContainsKey(MetadataGraphUri))
                {
                    triplesByGraphUri[MetadataGraphUri] = (new List<string>(), new List<string>());
                }

                foreach (var graph in graphs)
                { 
                    var triplesToModify = parameters.Select(parameter => $"{graph.AbsoluteUri} , {qualityCheckPredicate} , {parameter}").ToList();
                    if (operationToPerform == OperationToPerform.AddQualityChecks)
                    {
                        foreach (var triple in triplesToModify)
                        {
                            triplesByGraphUri[MetadataGraphUri].TriplesToAdd.Add(triple);
                        }
                    }
                    else
                    {
                        foreach (var triple in triplesToModify)
                        {
                            triplesByGraphUri[MetadataGraphUri].TriplesToRemove.Add(triple);
                        }
                    }
                }
            }

            return await triplestoreClientQualityWrapper.UpdateGraphs(dataset, triplesByGraphUri);
        }
    }
}
