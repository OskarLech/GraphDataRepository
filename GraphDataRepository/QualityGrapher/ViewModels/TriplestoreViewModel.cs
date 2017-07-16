using System;
using Libraries.Server;
using QualityGrapher.Models;
using QualityGrapher.Views;
using static Libraries.Server.SupportedTriplestores;

namespace QualityGrapher.ViewModels
{
    public class TriplestoreViewModel : ViewModelBase
    {
        public TriplestoreModel TriplestoreModel { get; set; }
        public SupportedOperation SelectedOperation { get; set; }

        public TriplestoreViewModel()
        {
            ((MainWindow)System.Windows.Application.Current.MainWindow).LanguageSet += delegate { OnPropertyChanged(nameof(TriplestoreModel)); };
        }

        public void CreateTriplestoreQualityWrapper(string endpointUri)
        {
            var triplestoreClient = (ITriplestoreClient) Activator.CreateInstance(TriplestoreModel.Type, endpointUri);
            TriplestoreModel.TriplestoreClientQualityWrapper = new TriplestoreClientQualityWrapper(triplestoreClient); 
        }

        public override string ToString()
        {
            return TriplestoreModel.Name;
        }
    }
}
