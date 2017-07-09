using System;
using System.Collections.Concurrent;
using System.Windows;
using QualityGrapher.Views;
using Serilog.Core;
using Serilog.Events;

namespace QualityGrapher.Utilities.Serilog
{
    internal class TextBoxSink : ILogEventSink
    {
        //private readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter("{Timestamp} [{Level}] {Message}", new TextBoxSinkFormatProvider());
        private readonly IFormatProvider _formatProvider = new TextBoxSinkFormatProvider();
        private readonly MainWindow _mainWindow = (MainWindow)Application.Current.MainWindow;

        public ConcurrentQueue<string> Events { get; } = new ConcurrentQueue<string>();

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            _mainWindow.LogBox.Dispatcher.BeginInvoke((Action)(() => _mainWindow.LogBox.Text = logEvent.RenderMessage(_formatProvider)));
        }
    }
}