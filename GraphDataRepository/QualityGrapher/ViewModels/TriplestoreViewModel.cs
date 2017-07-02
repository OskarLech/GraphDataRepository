﻿using System.Collections.Generic;
using QualityGrapher.Views;
using static Libraries.Server.SupportedTriplestores;

namespace QualityGrapher.ViewModels
{
    public class TriplestoreViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public IEnumerable<SupportedOperations> SupportedOperations { get; set; }

        public SupportedOperations SelectedOperation { get; set; }

        public TriplestoreViewModel()
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).LanguageSet += delegate { OnPropertyChanged(nameof(SupportedOperations)); };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}