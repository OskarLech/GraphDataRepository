using System.Collections.Generic;
using VDS.RDF;

namespace Libraries.QualityChecks
{
    public interface IQualityCheck
    {
        QualityCheckReport CheckGraphs(IEnumerable<IGraph> graphs, IEnumerable<object> parameters);
        QualityCheckReport CheckData(IEnumerable<Triple> triples, IEnumerable<object> parameters); 
        IEnumerable<string> GetParameters();
        void CancelCheck();
        string GetPredicate();
    }
}
