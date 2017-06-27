using System.Collections.Generic;
using QualityGrapher.Models;
using QualityGrapher.Utilities;

namespace QualityGrapher.ViewModels
{
    internal class TriplestoresViewModel : ViewModelBase
    {
        public List<TriplestoreModel> Triplestores { get; } = SupportedTriplestores.Instance.TriplestoreModelList;

        public TriplestoresViewModel()
        {
        }
    }
}
