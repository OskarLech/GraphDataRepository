using System;
using System.Collections.Generic;
using Libraries.QualityChecks;
using QualityGrapher.Views;
using static Libraries.QualityChecks.QualityChecksData;

namespace QualityGrapher.ViewModels
{
    public class QualityCheckListViewModel : ViewModelBase
    {
        public IEnumerable<(string name, Type qualityCheckClass)> SupportedQualityCheckList { get; private set; } = SupportedQualityChecks;
        public IQualityCheck SelectedQualityCheck { get; set; }

        public QualityCheckListViewModel()
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).LanguageSet += delegate { OnPropertyChanged(nameof(SelectedQualityCheck)); };
        }
    }
}
