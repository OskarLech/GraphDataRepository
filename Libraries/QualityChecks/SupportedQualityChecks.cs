using System;
using System.Collections.Generic;
using System.Linq;

namespace Libraries.QualityChecks
{
    public static class QualityChecksData
    {
        public const string WholeDatasetSubject = "WholeDataset";
        public static readonly Uri MetadataGraphUri = new Uri("resources://metadata");
        public static readonly IEnumerable<(string name, Type qualityCheckClass)> SupportedQualityChecks;
        public static readonly IEnumerable<IQualityCheck> QualityCheckInstances;

        static QualityChecksData()
        {
            SupportedQualityChecks = new List<(string, Type)>
            {
                ("KnowledgeBase check", typeof(KnowledgeBaseCheck.KnowledgeBaseCheck)),
                ("Vocabulary check", typeof(VocabularyCheck.VocabularyCheck))
            };

            QualityCheckInstances = SupportedQualityChecks.Select(qualityCheck => (IQualityCheck)Activator.CreateInstance(qualityCheck.qualityCheckClass))
                .ToList();
        }
    }
}