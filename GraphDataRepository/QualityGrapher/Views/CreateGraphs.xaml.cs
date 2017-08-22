using System;
using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for CreateGraphs.xaml
    /// </summary>
    public partial class CreateGraphs : UserControl
    {
        private readonly ListDatasets _listDatasetsUserControl = new ListDatasets();

        public CreateGraphs()
        {
            InitializeComponent();
            ListDatasetsControl.Content = _listDatasetsUserControl;
        }

        private async void AddGraphButton_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            if (triplestoreClientQualityWrapper == null)
            {
                mainWindow.OnOperationFailed();
                return;
            }

            var dataset = _listDatasetsUserControl.DatasetListBox.SelectedItem?.ToString();

            Uri.TryCreate(GraphUriTextBox.Text, UriKind.RelativeOrAbsolute, out var graphUri);
            if (string.IsNullOrWhiteSpace(dataset) || !await triplestoreClientQualityWrapper.CreateGraph(dataset, graphUri))
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
