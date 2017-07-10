using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        private void ListDatasets_OnLoaded(object sender, EventArgs e)
        {
            GetDatasetList(DataContext);
        }

        public async void GetDatasetList(object dataContext)
        {
            if (DataContext == null)
            {
                DataContext = dataContext;
            }

            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            if (triplestoreClientQualityWrapper == null)
            {
                mainWindow.OnOperationFailed();
                return;
            }

            var datasets = await triplestoreClientQualityWrapper.ListDatasets();
            if (datasets != null)
            {
                DatasetListBox.ItemsSource = datasets;
            }
            else
            {
                mainWindow.OnOperationFailed();
            }
        }
    }
}
