using System.Collections.Generic;
using VDS.RDF;

namespace Libraries.QualityChecks
{
    public interface IQualityCheck
    {
        QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters);
        QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<object> parameters); 
        void CancelCheck();
        string GetPredicate();
    }
}
