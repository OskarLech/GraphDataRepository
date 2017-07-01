using System.Collections.Generic;
using QualityGrapher.Utilities;

namespace QualityGrapher.ViewModels
{
    internal class TriplestoresViewModel : ViewModelBase
    {
        public List<TriplestoreViewModel> Triplestores { get; } = SupportedTriplestores.Instance.TriplestoreModelList;
    }
}
