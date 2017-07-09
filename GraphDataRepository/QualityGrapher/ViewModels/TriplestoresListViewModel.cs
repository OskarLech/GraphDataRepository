using System.Collections.Generic;
using System.Linq;
using Libraries.Server;
using QualityGrapher.Models;

namespace QualityGrapher.ViewModels
{
    internal class TriplestoresListViewModel : ViewModelBase
    {
        public string EndpointUri { set; get; }
        public List<TriplestoreViewModel> Triplestores { get; } = PopulateTriplestoreList();
        private TriplestoreViewModel _selectedTriplestore;

        public TriplestoreViewModel SelectedTriplestore
        {
            get => _selectedTriplestore;
            set
            {
                _selectedTriplestore = value;
                _selectedTriplestore.CreateTriplestoreQualityWrapper(EndpointUri);
            }
        }

        private static List<TriplestoreViewModel> PopulateTriplestoreList()
        {
            return SupportedTriplestores.GetTriplestoresList()
                .Select(triplestore => new TriplestoreViewModel
                {
                    TriplestoreModel = new TriplestoreModel
                    {
                        Name = triplestore.Name,
                        Type = triplestore.Type,
                        SupportedOperations = triplestore.SupportedOperations
                    }
                })
                .ToList();
        }
    }
}
