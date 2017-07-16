using System;
using System.Collections.Generic;

namespace Libraries.QualityChecks
{
    public static class QualityChecksData
    {
        public const string WholeDatasetSubject = "WholeDataset";
        public static readonly Uri MetadataGraphUri = new Uri("resources://metadata");

        public static IEnumerable<(string name, Type qualityCheckClass)> SupportedQualityChecks = new List<(string, Type)>
        {
            ("KnowledgeBase check", typeof(KnowledgeBaseCheck.KnowledgeBaseCheck)),
            ("Vocabulary check", typeof(VocabularyCheck.VocabularyCheck))
        };
    }
}
