﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QualityGrapher.Views
{
    /// <summary>
    /// Interaction logic for DeleteGraphs.xaml
    /// </summary>
    public partial class DeleteGraphs : UserControl
    {
        private readonly ListGraphs _listGraphsUserControl = new ListGraphs();

        public DeleteGraphs()
        {
            InitializeComponent();
            ListGraphsControl.Content = _listGraphsUserControl;
        }

        private async void DeleteGraphsButton_OnClick(object sender, RoutedEventArgs e)
        {
            var triplestoreClientQualityWrapper = UserControlHelper.GetTriplestoreClientQualityWrapper(DataContext);
            var mainWindow = (MainWindow)Application.Current.MainWindow;

            var dataset = UserControlHelper.GetDatasetFromListDatasetsUserControl(_listGraphsUserControl.ListDatasetsControl);
            var graphs = _listGraphsUserControl.ListGraphsListBox.SelectedItems.Cast<Uri>();
            if (triplestoreClientQualityWrapper == null || string.IsNullOrWhiteSpace(dataset) || !await triplestoreClientQualityWrapper.DeleteGraphs(dataset, graphs))
            {
                mainWindow.OnOperationFailed();
            }
            else
            {
                mainWindow.OnOperationSucceeded();
            }
        }
    }
}
