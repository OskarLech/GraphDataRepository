using System;
using Serilog;
using Serilog.Configuration;

namespace QualityGrapher.Utilities.Serilog
{
    public static class TextBoxSinkExtensions
    {
        public static LoggerConfiguration TextBoxSink(this LoggerSinkConfiguration loggerConfiguration, IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new TextBoxSink());
        }
    }
}
