using Libraries.Server;
using QualityGrapher.ViewModels;

namespace QualityGrapher.Views
{
    public static class UserControlHelper
    {
        public static ITriplestoreClientQualityWrapper GetTriplestoreClientQualityWrapper(object dataContext)
        {
            return ((TriplestoresListViewModel)dataContext)?.SelectedTriplestore?.TriplestoreModel?.TriplestoreClientQualityWrapper;
        }
    }
}
