using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for CreateDataset.xaml
    /// </summary>
    public partial class CreateDataset : UserControl
    {
        public CreateDataset()
        {
            InitializeComponent();
        }

        private async void CreateDatasetButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow) Application.Current.MainWindow;
            if (triplestoreClientQualityWrapper == null || !await triplestoreClientQualityWrapper.CreateDataset(CreateDatasetTextBox.Text))
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
