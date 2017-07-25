using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using static Libraries.QualityChecks.QualityChecksData;

namespace QualityGrapher.Converters
{
    public class QualityCheckToTextConverter : LanguageConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var qualityChecksToConvert = value as IEnumerable;
            if (qualityChecksToConvert == null) return new List<string>();

            var qualityChecksTextList = new List<string>();
            foreach (var qualityCheck in qualityChecksToConvert)
            {
                switch (qualityCheck)
                {
                    case nameof(SupportedQualityCheck.KnowledgeBaseCheck):
                        qualityChecksTextList.Add(ResourceDictionary[nameof(SupportedQualityCheck.KnowledgeBaseCheck)].ToString());
                        break;

                    case nameof(SupportedQualityCheck.VocabularyCheck):
                        qualityChecksTextList.Add(ResourceDictionary[nameof(SupportedQualityCheck.VocabularyCheck)].ToString());
                        break;
                }
            }

            return qualityChecksTextList;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
