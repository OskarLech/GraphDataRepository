using System.Collections.Generic;
using QualityGrapher.Globalization.Resources;
using QualityGrapher.Utilities;
using QualityGrapher.Utilities.StructureMap;
using QualityGrapher.Views;

namespace QualityGrapher.ViewModels
{
    public class TriplestoreViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public Dictionary<SupportedOperations, string> SupportedOperations { get; set; } //{operation, displayed text}

        private readonly DynamicData _dynamicData = ObjectFactory.Container.GetInstance<DynamicData>();

        public TriplestoreViewModel()
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).LanguageSet += OnLanguageSet;
        }

        private void OnLanguageSet(string obj)
        {
            var supportedOperations = new Dictionary<SupportedOperations, string>();
            foreach (var operation in SupportedOperations.Keys)
            {
                supportedOperations[operation] = _dynamicData.GetTriplestoreOperationText(operation);
            }

            SupportedOperations = supportedOperations;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
