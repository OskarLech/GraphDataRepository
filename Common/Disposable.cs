using System;
using log4net;

namespace Common
{
    public abstract class Disposable : IDisposable
    {
        private bool _disposed;
        protected ILog Log { get; }
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
            Log.Debug($"Disposing {GetType().Name}...");
            Dispose(true);
            Log.Debug($"{GetType().Name} disposed");
            GC.SuppressFinalize(this);
        }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}
