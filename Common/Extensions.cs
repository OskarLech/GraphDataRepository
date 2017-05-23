using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;

namespace Common
{
    public static class Extensions
    {
        public static string GetDetails(this Exception e)
        {
            var details = $"{e.GetType().Name}: {e.Message}, Stack trace:{e.StackTrace}" + (e.InnerException != null ? "\n\nInner Exception: " + e.InnerException.GetDetails() : "");

            var webEx = e as WebException;
            if (webEx != null)
            {
                details += $"\nWebException details:\n\nResponse: {webEx.Response}, Status: {webEx.Status}";
                var responseStream = webEx.Response.GetResponseStream();
                if (responseStream != null)
                {
                    details += $", Reponse: {new StreamReader(responseStream).ReadToEnd()}";
                }
            }

            return details;
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

        public static IEnumerable<T> SingleItemAsEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}
