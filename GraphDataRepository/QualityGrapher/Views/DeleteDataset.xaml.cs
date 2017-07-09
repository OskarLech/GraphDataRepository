using System.Windows;
using System.Windows.Controls;
using Libraries.Server;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for DeleteDataset.xaml
    /// </summary>
    public partial class DeleteDataset : UserControl
    {
        public DeleteDataset()
        {
            InitializeComponent();
        }

        private async void DeleteDatasetButton_OnClick(object sender, RoutedEventArgs e)
        {

            var triplestoreClientQualityWrapper = (ITriplestoreClientQualityWrapper)DataContext;
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (await triplestoreClientQualityWrapper.DeleteDataset(DeleteDatasetTextBox.Text))
            {
                mainWindow.OnOperationSucceeded();
            }
            else
            {
                mainWindow.OnOperationFailed();
            }
        }
    }
}
