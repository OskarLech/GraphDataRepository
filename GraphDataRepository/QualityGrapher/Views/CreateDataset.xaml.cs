using System;
using System.Windows;
using Libraries.Server;
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
            var triplestoreClientQualityWrapper = (ITriplestoreClientQualityWrapper) DataContext;
            var mainWindow = (MainWindow) Application.Current.MainWindow;
            if (await triplestoreClientQualityWrapper.CreateDataset(CreateDatasetTextBox.Text))
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
