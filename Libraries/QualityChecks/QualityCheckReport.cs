using System.Collections.Generic;

namespace Libraries.QualityChecks
{
    /// <summary>
    /// Indicates if quality check passed and lists errors
    /// </summary>
    public class QualityCheckReport
    {
        public bool QualityCheckPassed { get; set; }

        //{Id, (graph URI, triple, error message)}
        public readonly Dictionary<int, (string GraphUri, string Triple, string ErrorMessage)> ErrorsById = new Dictionary<int, (string, string, string)>();
    }
}