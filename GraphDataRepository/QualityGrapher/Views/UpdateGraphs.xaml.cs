using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for UpdateGraphs.xaml
    /// </summary>
    public partial class UpdateGraphs : UserControl
    {
        private readonly ListGraphs _listGraphsUserControl = new ListGraphs();
        private readonly QualityCheckControl _qualityCheckControl = new QualityCheckControl();

        public UpdateGraphs()
        {
            InitializeComponent();
            ListGraphsControl.Content = _listGraphsUserControl;
            QualityCheckControl.Content = _qualityCheckControl;
        }

        private async void UpdateGraphsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var dataset = UserControlHelper.GetDatasetFromListDatasetsUserControl(_listGraphsUserControl.ListDatasetsControl);
            var graphs = _listGraphsUserControl.ListGraphsListBox.SelectedItems.Cast<Uri>();

            IEnumerable<string> triplesToRemove = TriplesToRemoveTextbox.Text.Split("\n".ToCharArray()).ToList();
            IEnumerable<string> triplesToAdd = TriplesToAddTextbox.Text.Split("\n".ToCharArray()).ToList();
            var triplesByGraphUri = graphs.ToDictionary(graph => graph, graph => (triplesToAdd, triplesToRemove));

            if(triplestoreClientQualityWrapper == null || string.IsNullOrWhiteSpace(dataset) || !await triplestoreClientQualityWrapper.UpdateGraphs(dataset, triplesByGraphUri))
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
