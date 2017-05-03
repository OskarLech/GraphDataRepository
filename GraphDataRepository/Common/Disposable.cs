using System;
using log4net;

namespace GraphDataRepository.Common
{
    internal abstract class Disposable : IDisposable
    {
        private bool _disposed;
        protected ILog Log { get; private set; }
        protected Disposable(ILog log)
        {
            Log = log;
        }

        protected abstract void OnDispose(bool disposing);
        public void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            OnDispose(disposing);
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}
