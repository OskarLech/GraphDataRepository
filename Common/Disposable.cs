using System;
using static Serilog.Log;

namespace Common
{
    public abstract class Disposable : IDisposable
    {
        private bool _disposed;

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
            Logger.Debug($"Disposing {GetType().Name}...");
            Dispose(true);
            Logger.Debug($"{GetType().Name} disposed");
            GC.SuppressFinalize(this);
        }

        ~Disposable()
        {
            Dispose(false);
        }
    }
}
