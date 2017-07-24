using System.Windows;
using System.Windows.Controls;
using QualityGrapher.ViewModels;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for QualityCheckControl.xaml
    /// </summary>
    public partial class QualityCheckControl : UserControl
    {
        public QualityCheckControl()
        {
            InitializeComponent();
        }

        private void QualityCheckListBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            QualityCheckComboBox.DataContext = new QualityCheckListViewModel();
        }
    }
}
