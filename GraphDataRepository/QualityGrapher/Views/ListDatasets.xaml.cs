using System.Windows;
using System.Windows.Controls;
using Libraries.Server;
using QualityGrapher.ViewModels;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for ListDatasets.xaml
    /// </summary>
    public partial class ListDatasets : UserControl
    {
        public ListDatasets()
        {
            InitializeComponent();
        }

        private async void ListDatasets_OnLoaded(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = ((TriplestoresListViewModel)DataContext).SelectedTriplestore.TriplestoreModel.TriplestoreClientQualityWrapper;
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            var datasets = await triplestoreClientQualityWrapper.ListDatasets();
            if (datasets != null)
            {
                mainWindow.OnOperationSucceeded();
                DatasetListTextBox.Text = datasets.ToString();
            }
            else
            {
                mainWindow.OnOperationFailed();
            }
        }
    }
}
