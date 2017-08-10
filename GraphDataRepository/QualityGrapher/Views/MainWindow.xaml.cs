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
            PaintLogArea(LogBox.Background = Brushes.Green); ;
        }

        private async void PaintLogArea(Brush brush)
        {
            LogBox.Background = brush;
            await Task.Delay(2000);

            LogBox.Text = string.Empty;
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
            var dict = new ResourceDictionary();
            switch (language)
            {
                case SupportedLanguages.English:
                    dict.Source = new Uri(@"..\Globalization\Resources\en-GB.xaml", UriKind.Relative);
                    break;
                case SupportedLanguages.Polish:
                    dict.Source = new Uri(@"..\Globalization\Resources\pl-PL.xaml", UriKind.Relative);
                    break;
                default:
                    language = SupportedLanguages.English;
                    dict.Source = new Uri(@"..\Globalization\Resources\en-GB.xaml", UriKind.Relative);
                    break;
            }

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(dict);

            LanguageSet?.Invoke(language);
            ResetIndexOnOperationSelectionComboBox();
        }

        #region testing
        private void TestBtn_OnClick_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void VocabQualityCheckBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Log.Verbose("VocabQualityCheckBtn_OnClick");
            var dataGraph = new Graph();
            FileLoader.Load(dataGraph, @"..\..\..\..\Common\TestData\RDF\foaf_example.rdf");
            var vocabPath = Path.GetFullPath(@"..\..\..\..\Common\TestData\Schemas\foaf_20140114.rdf");
            var vocabCheck = new VocabularyCheck();
            vocabCheck.CheckGraphs(dataGraph.AsEnumerable(), new Uri(vocabPath).AsEnumerable());
        }

        private void KnowledgeBaseBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var dataGraph = new Graph();
            FileLoader.Load(dataGraph, @"..\..\..\..\Common\TestData\RDF\foaf_example.rdf");

            //var query = "SELECT DISTINCT ?concept\r\nWHERE {\r\n    <http://dbpedia.org/resource/NASA> a ?concept\r\n    FILTER ( strstarts(str(?concept), \"http://dbpedia.org/class/yago/\") )\r\n}\r\nLIMIT 1";
            var knowledgeBaseCheck = new KnowledgeBaseCheck();
            var parameters = (object)new ValueTuple<Uri, Uri, string>(new Uri("http://dbpedia.org/sparql"), null, "");
            knowledgeBaseCheck.CheckGraphs(dataGraph.AsEnumerable(), parameters.AsEnumerable());
        }

        private async void BrightstarCliBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClient = new BrightstarClient("http://192.168.0.3:8090/brightstar");

            if (triplestoreClient is IDisposable disposable)
            {
                Disposables.Add(disposable);
            }

            while (true)
            {
                Console.WriteLine("\n\nESC - exit");
                Console.WriteLine("F1 - Create dataset");
                Console.WriteLine("C - Delete dataset");
                Console.WriteLine("F3 - List datasets");
                Console.WriteLine("F4 - Delete graph");
                Console.WriteLine("F5 - Update graph");
                Console.WriteLine("F6 - Read graph");
                Console.WriteLine("F7 - List graphs");
                Console.WriteLine("F8 - Run SPARQL query\n\n");

                string dataset;
                string graph;
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.F1:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        if (await triplestoreClient.CreateDataset(dataset))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.C:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        if (await triplestoreClient.DeleteDataset(dataset))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.F3:
                        foreach (var ds in await triplestoreClient.ListDatasets())
                        {
                            Console.WriteLine(ds);
                        }
                        break;

                    case ConsoleKey.F4:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("graph URI:");
                        graph = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(graph)) continue;
                        if (await triplestoreClient.DeleteGraphs(dataset, new List<Uri> { new Uri(graph) }))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.F5: //update graph
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("graph URI:");
                        graph = Console.ReadLine();

                        var triplesToRemove = new List<string>();
                        var triplesToAdd = new List<string>();

                        triplesToRemove.Add("<xhttp://www.w3.org/2001/sw/RDFCore/ntriples/> <xhttp://www.w3.org/1999/02/22-rdf-syntax-ns#type> <xhttp://xmlns.com/foaf/0.1/Document>");
                        triplesToRemove.Add("<xhttp://www.w3.org/2001/sw/RDFCore/ntriples/> <xhttp://www.w3.org/1999/02/22-rdf-syntax-ns#type> <xhttp://xmlns.com/foaf/0.1/Document>");
                        triplesToRemove.Add("<xhttp://www.w3.org/2001/sw/RDFCore/ntriples/> <xhttp://www.w3.org/1999/02/22-rdf-syntax-ns#type> <xhttp://xmlns.com/foaf/0.1/Document>");
                        triplesToAdd.Add("<http://www.w3.org/2001/sw/RDFCore/ntriples/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/Document>");
                        triplesToAdd.Add("<http://www.w3.org/2001/sw/RDFCore/ntriples/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/Document>");
                        triplesToAdd.Add("<http://www.w3.org/2001/sw/RDFCore/ntriples/> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://xmlns.com/foaf/0.1/Document>");

                        if (string.IsNullOrWhiteSpace(graph)) continue;
                        var triplesByUri = new Dictionary<Uri, (IEnumerable<string> triplesToRemove, IEnumerable<string> triplesToAdd)>
                        {
                            [new Uri(graph)] = (triplesToRemove, triplesToAdd)
                        };

                        if (await triplestoreClient.UpdateGraphs(dataset, triplesByUri))
                        {
                            Console.WriteLine("success");
                        }
                        break;

                    case ConsoleKey.F6:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("graph URI:");
                        graph = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(graph)) continue;
                        var dnrGraph = await triplestoreClient.ReadGraphs(dataset, new Uri(graph).AsEnumerable());
                        break;

                    case ConsoleKey.F7:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        foreach (var g in await triplestoreClient.ListGraphs(dataset))
                        {
                            Console.WriteLine(g);
                        }
                        break;

                    case ConsoleKey.F8:
                        Console.WriteLine("dataset name:");
                        dataset = Console.ReadLine();
                        Console.WriteLine("query:");
                        var query = Console.ReadLine();
                        var graphs = new List<Uri>
                        {
                            new Uri("http://example.org/graph1"),
                            new Uri("http://example.org/graph2")
                        };

                        var resultSet = await triplestoreClient.RunSparqlQuery(dataset, graphs, query);
                        foreach (VDS.RDF.Query.SparqlResult result in resultSet)
                        {
                            Console.WriteLine(result.ToString());
                        }
                        break;
                }

                if (key.Key == ConsoleKey.Escape) break;
            }
        }

        #endregion

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
