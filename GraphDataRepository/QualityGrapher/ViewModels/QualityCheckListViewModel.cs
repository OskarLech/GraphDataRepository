using System;
using System.Collections.Generic;
using QualityGrapher.Views;
using static Libraries.QualityChecks.QualityChecksData;

namespace QualityGrapher.ViewModels
{
    public class QualityCheckListViewModel : ViewModelBase
    {
        public IEnumerable<string> SupportedQualityCheckList { get; private set; } = Enum.GetNames(typeof(SupportedQualityCheck));

        public SupportedQualityCheck SelectedQualityCheck { get; set; }

        public QualityCheckListViewModel()
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).LanguageSet += delegate { OnPropertyChanged(nameof(SelectedQualityCheck)); };
        }
    }
}
