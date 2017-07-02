using System.Collections.Generic;
using Libraries.Server;
using QualityGrapher.Utilities;

namespace QualityGrapher.ViewModels
{
    internal class TriplestoresViewModel : ViewModelBase
    {
        public List<TriplestoreViewModel> Triplestores { get; } = SupportedTriplestores.Instance.TriplestoreModelList;
        public TriplestoreViewModel SelectedTriplestore { get; set; }
    }
}
