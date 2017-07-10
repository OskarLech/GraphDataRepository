using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for DeleteGraphs.xaml
    /// </summary>
    public partial class DeleteGraphs : UserControl
    {
        public DeleteGraphs()
        {
            InitializeComponent();
        }

        private void DeleteGraphButton_OnClick(object sender, RoutedEventArgs e)
        {
        //    var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
        //    var mainWindow = (MainWindow)Application.Current.MainWindow;

        //    var dupa = DeleteGraphTextBox.Text.Split("\n".ToCharArray()).ToList();

        //    if (triplestoreClientQualityWrapper == null || !await triplestoreClientQualityWrapper.DeleteGraphs())
        //    {
        //        mainWindow.OnOperationFailed();
        //    }
        //    else
        //    {
        //        mainWindow.OnOperationSucceeded();
        //        ListDatasets();
        //    }
        }
    }
}
