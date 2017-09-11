using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Libraries.QualityChecks.KnowledgeBaseCheck;
using Libraries.QualityChecks.VocabularyCheck;
using Libraries.Server.BrightstarDb;
using QualityGrapher.Globalization;
using QualityGrapher.Utilities.Serilog;
using Serilog;
using VDS.RDF;
using VDS.RDF.Parsing;
using QualityGrapher.ViewModels;
using static Serilog.Log;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public event Action<string> LanguageSet;
        private static readonly List<IDisposable> Disposables = new List<IDisposable>();

        public MainWindow()
        {
            InitializeComponent();
            Init();
            InitBindings();
        }

        public void OnOperationFailed()
        {
            PaintLogArea(LogBox.Background = Brushes.Red);
        }

        public void OnOperationSucceeded()
        {
            LogBox.Text = string.Empty;
            PaintLogArea(LogBox.Background = Brushes.Green);
        }

        private async void PaintLogArea(Brush brush)
        {
            LogBox.Background = brush;
            await Task.Delay(2000);
            LogBox.Background = Brushes.White;
        }

        private void InitBindings()
        {
            var triplestoresViewModel = new TriplestoresListViewModel();
            EndpointUriTextBox.DataContext = triplestoresViewModel;
            ServerSelectionComboBox.DataContext = triplestoresViewModel;
            OperationSelectionComboBox.DataContext = triplestoresViewModel;
            TriplestoreOperationUserControl.DataContext = triplestoresViewModel;
        }

        private void Init()
        {
            //Serilog
            Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.TextBoxSink() //TODO move to app.config
                .CreateLogger();

            //Languages
            SetLanguageDictionary(Thread.CurrentThread.CurrentCulture.ToString());
        }

        private void SetLanguageDictionary(string language)
        {
            if (!SupportedLanguages.IsLanguageSupported(language))
            {
                language = SupportedLanguages.Default;
            }

            var dict = new ResourceDictionary {Source = SupportedLanguages.GetResourceDictionary(language)};

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(dict);

            LanguageSet?.Invoke(language);
            ResetIndexOnOperationSelectionComboBox();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Verbose("Terminating the program");
            Disposables.ForEach(d => d.Dispose());
            Verbose("Program terminated succesfully");
        }

        private void PolishLanguageButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary(SupportedLanguages.Polish);
        }

        private void EnglishLanguageButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetLanguageDictionary(SupportedLanguages.English);
        }

        private void ServerSelectionComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResetIndexOnOperationSelectionComboBox();
            MakeEndpointUriTextBoxReadonly();
        }

        private void MakeEndpointUriTextBoxReadonly()
        {
            EndpointUriTextBox.IsReadOnly = true;
            EndpointUriTextBox.Background = Brushes.LightGray;
        }

        /// <summary>
        /// ComboBox text is not updated automatically if binding updates
        /// </summary>
        private void ResetIndexOnOperationSelectionComboBox()
        {
            OperationSelectionComboBox.SelectedIndex = 0;
        }
    }
}
