using System.Windows;
using System.Windows.Controls;
using QualityGrapher.ViewModels;
using ListBox = System.Windows.Forms.ListBox;

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
            QualityCheckComboBox.SelectedIndex = 0;
        }

        private void AddQualityCheckButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void RemoveQualityCheckButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
