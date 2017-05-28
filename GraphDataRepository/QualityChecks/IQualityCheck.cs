using System.Collections.Generic;
using VDS.RDF;

namespace GraphDataRepository.QualityChecks
{
    public interface IQualityCheck
    {
        QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<string> parameters = null);
        QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<IGraph> graphs = null, IEnumerable<string> parameters = null); 
        void FixErrors(QualityCheckReport qualityCheckReport, string dataset, IEnumerable<int> errorsToFix);
        IEnumerable<string> ListParameters();
        bool ImportParameters(IEnumerable<object> parameters);
        void CancelCheck();
    }
}
