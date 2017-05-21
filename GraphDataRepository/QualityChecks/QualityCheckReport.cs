using System;
using System.Collections.Generic;

namespace GraphDataRepository.QualityChecks
{
    /// <summary>
    /// Indicates if quality check passed and lists errors
    /// </summary>
    public class QualityCheckReport
    {
        public bool QualityCheckPassed { get; set; } = true;

        //Key: ID, value: graph URI or triple, error message and flag indicating if it's possible to fix the error automatically
        public readonly Dictionary<int, IEnumerable<Tuple<string, string, bool>>> ErrorsById = new Dictionary<int, IEnumerable<Tuple<string, string, bool>>>();
    }
}