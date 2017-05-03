using Common;
using log4net;

namespace GraphDataRepository.Server.BrightstarDb
{
    internal class BrightstarClient : Disposable, ITriplestoreClient
    {
        public BrightstarClient(ILog log) : base(log)
        {
        }

        protected override void OnDispose(bool disposing)
        {
            throw new System.NotImplementedException();
        }
    }
}
