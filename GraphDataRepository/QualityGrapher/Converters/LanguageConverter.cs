﻿using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using QualityGrapher.Globalization;
using QualityGrapher.Views;

namespace QualityGrapher.Converters
{
    public abstract class LanguageConverter : IValueConverter
    {
        protected ResourceDictionary ResourceDictionary;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        protected LanguageConverter()
        {
            SetResourceDictionary(Thread.CurrentThread.CurrentCulture.ToString());
            ((MainWindow)Application.Current.MainWindow).LanguageSet += OnLanguageSet;
        }

        private void OnLanguageSet(string language)
        {
            SetResourceDictionary(language);
        }

        private void SetResourceDictionary(string language)
        {
            ResourceDictionary = new ResourceDictionary
            {
                Source = SupportedLanguages.GetResourceDictionary(language)
            };
        }
    }
}
