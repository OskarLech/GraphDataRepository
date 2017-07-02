using System.Collections.Generic;
using System.Linq;
using Libraries.Server;

namespace QualityGrapher.ViewModels
{
    internal class TriplestoresViewModel : ViewModelBase
    {
        public List<TriplestoreViewModel> Triplestores { get; } = PopulateTriplestoreList();

        public TriplestoreViewModel SelectedTriplestore { get; set; }

        private static List<TriplestoreViewModel> PopulateTriplestoreList()
        {
            return SupportedTriplestores.GetTriplestoresList()
                .Select(triplestore => new TriplestoreViewModel
                {
                    Name = triplestore.name,
                    SupportedOperations = triplestore.supportedOperations
                })
                .ToList();
        }
    }
}
