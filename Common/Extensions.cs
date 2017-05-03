using System;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace Common
{
    public static class Extensions
    {
        public static string GetDetails(this Exception e)
        {
            return $"{e.GetType().Name}: {e.Message}, ST:{e.StackTrace}" + (e.InnerException != null ? "\n\nInner Exception: " + e.InnerException.GetDetails() : "");
        }

        public static void WaitAndLog(this Task task, int seconds, string msg)
        {
            var log = LogManager.GetLogger(typeof(Extensions));
            if (task == null)
            {
                log.Warn("WaitAndLog extension method called on a null task");
                return;
            }

            try
            {
                if (!task.Wait(TimeSpan.FromSeconds(seconds)))
                {
                    log.Warn(msg);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions.Any(e => !(e is TaskCanceledException) && !(e is OperationCanceledException)))
                {
                    throw;
                }
            }
        }
    }
}
