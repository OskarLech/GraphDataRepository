﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Libraries.Server
{
    /// <summary>
    /// Interface for basic CRUD operations any triplestore should provide
    /// </summary>
    public interface ITriplestoreClientBasic : ITriplestoreSparql
    {
        Task<bool> CreateDataset(string name);
        Task<bool> DeleteDataset(string name);
        Task<IEnumerable<string>> ListDatasets();
        Task<bool> DeleteGraphs(string dataset, IEnumerable<Uri> graphUris);
        Task<bool> UpdateGraphs(string dataset, Dictionary<Uri, (IList<string> TriplesToRemove, IList<string> TriplesToAdd)> triplesByGraphUri);
        Task<bool> CreateGraph(string dataset, Uri graphUri);
        void CancelOperation();
    }
}
