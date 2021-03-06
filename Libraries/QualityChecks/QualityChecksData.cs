﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Libraries.QualityChecks
{
    public static class QualityChecksData
    {
        public static readonly Uri WholeDatasetSubjectUri = new Uri("resource://wholedataset");
        public static readonly Uri MetadataGraphUri = new Uri("resource://metadata");
        public static readonly IEnumerable<IQualityCheck> QualityCheckInstances;

        public enum SupportedQualityCheck
        {
            KnowledgeBaseCheck,
            VocabularyCheck
        }

        static QualityChecksData()
        {
            var supportedQualityChecks = new List<(SupportedQualityCheck Name, Type QualityCheckClass)>
            {
                (SupportedQualityCheck.KnowledgeBaseCheck, typeof(KnowledgeBaseCheck.KnowledgeBaseCheck)),
                (SupportedQualityCheck.VocabularyCheck, typeof(VocabularyCheck.VocabularyCheck))
            };

            QualityCheckInstances = supportedQualityChecks.Select(qualityCheck => (IQualityCheck)Activator.CreateInstance(qualityCheck.QualityCheckClass))
                .ToList();
        }

        public enum OperationToPerform
        {
            AddQualityChecks,
            RemoveQualityChecks
        }
    }
}
