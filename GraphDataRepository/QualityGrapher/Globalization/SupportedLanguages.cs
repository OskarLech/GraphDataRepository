using System;

namespace QualityGrapher.Globalization
{
    internal class SupportedLanguages
    {
        public const string Polish = "pl-PL";
        public const string English = "en-GB";
        public const string Default = English;

        private const string DictionaryPath = @"..\Globalization\Resources\";

        public static Uri GetResourceDictionary(string language)
        {
            string dictionaryLanguage; 
            switch (language)
            {
                case Polish:
                    dictionaryLanguage = Polish;
                    break;
                case English:
                    dictionaryLanguage = English;
                    break;
                default:
                    dictionaryLanguage = Default;
                    break;
            }

            return new Uri($"{DictionaryPath}{dictionaryLanguage}.xaml", UriKind.RelativeOrAbsolute);
        }

        public static bool IsLanguageSupported(string language)
        {
            switch (language)
            {
                case Polish:
                case English:
                    return true;
                default:
                    return false;
            }
        }
    }
}
