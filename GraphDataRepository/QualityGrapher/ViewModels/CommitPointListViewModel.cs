using System;
using System.Collections.Generic;

namespace QualityGrapher.ViewModels
{
    public class CommitPointListViewModel : ViewModelBase
    {
        public IEnumerable<(ulong Id, DateTime CommitTime)> CommitInfoList { get; set; }
        public (ulong Id, DateTime CommitTime) SelectedCommit { get; set; }
    }
}
