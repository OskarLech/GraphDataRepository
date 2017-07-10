using System;
using System.Collections.Generic;

namespace Libraries.QualityChecks
{
    public class QualityChecksData
    {
        public const string WholeDatasetSubject = "WholeDataset";
        public static readonly Uri MetadataGraphUri = new Uri("<http://www.brightstardb.com/companies/brightstardb>");

        public static IEnumerable<(string name, Type qualityCheckClass)> SupportedQualityChecks = new List<(string, Type)>
        {
            ("KnowledgeBase check", typeof(KnowledgeBaseCheck.KnowledgeBaseCheck)),
            ("Vocabulary check", typeof(VocabularyCheck.VocabularyCheck))
        };
    }
}
