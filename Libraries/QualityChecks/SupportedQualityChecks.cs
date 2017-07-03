using System;
using System.Collections.Generic;

namespace Libraries.QualityChecks
{
    public class SupportedQualityChecks
    {
        public static IEnumerable<(string name, Type qualityCheckClass)> QualityChecksList = new List<(string, Type)>
        {
            ("KnowledgeBase check", typeof(KnowledgeBaseCheck.KnowledgeBaseCheck)),
            ("Vocabulary check", typeof(VocabularyCheck.VocabularyCheck))
        };
    }
}
