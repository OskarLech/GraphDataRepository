using System;
using System.Collections.Generic;
using System.Linq;
using Libraries.Server;

namespace QualityGrapher.ViewModels
{
    internal class TriplestoresListViewModel : ViewModelBase
    {
        public List<TriplestoreViewModel> Triplestores { get; } = PopulateTriplestoreList();
        public string EndpointUri { get; set; }

        private TriplestoreViewModel _selectedTriplestore;

        public TriplestoreViewModel SelectedTriplestore
        {
            get { return _selectedTriplestore; }
            set
            {
                _selectedTriplestore = value;
                var triplestoreType = _selectedTriplestore.Type;
                var triplestoreClient = (ITriplestoreClient)Activator.CreateInstance(triplestoreType, EndpointUri);
                _selectedTriplestore.TriplestoreClientQualityWrapper = new TriplestoreClientQualityWrapper(triplestoreClient);
            }
        }

        private static List<TriplestoreViewModel> PopulateTriplestoreList()
        {
            return SupportedTriplestores.GetTriplestoresList()
                .Select(triplestore => new TriplestoreViewModel
                {
                    Name = triplestore.Name,
                    Type = triplestore.Type,
                    SupportedOperations = triplestore.SupportedOperations
                })
                .ToList();
        }
    }
}
