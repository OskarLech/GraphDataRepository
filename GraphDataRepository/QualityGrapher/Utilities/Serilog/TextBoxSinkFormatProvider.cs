using System;
using System.Globalization;

namespace QualityGrapher.Utilities.Serilog
{
    public class TextBoxSinkFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            return this;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is IFormattable argAsIFormattable)
            {
                return argAsIFormattable.ToString(format, CultureInfo.CurrentCulture);
            }

            return arg?.ToString() ?? string.Empty;
        }
    }
}
