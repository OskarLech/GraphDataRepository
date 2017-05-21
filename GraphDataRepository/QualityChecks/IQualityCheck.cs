using System;
using System.Collections.Generic;

namespace GraphDataRepository.QualityChecks
{
    public interface IQualityCheck
    {
        QualityCheckReport CheckGraphs(IEnumerable<Uri> graphs, IEnumerable<string> parameters = null);
        QualityCheckReport CheckData(IEnumerable<string> triples, IEnumerable<Uri> graphs = null, IEnumerable<string> parameters = null); 
        void FixErrors(QualityCheckReport checkReport, IEnumerable<int> errorsToFix);
    }
}
