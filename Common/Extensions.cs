using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VDS.RDF;
using static Serilog.Log;

namespace Common
{
    public static class Extensions
    {
        private enum TripleObjects
        {
            Subject,
            Predicate,
            Object
        }

        public static string GetDetails(this Exception e)
        {
            var details = $"{e.GetType().Name}: {e.Message}, Stack trace:{e.StackTrace}" + (e.InnerException != null ? "\n\nInner Exception: " + e.InnerException.GetDetails() : "");

            if (e is WebException webEx)
            {
                details += $"\nWebException details:\n\nResponse: {webEx.Response}, Status: {webEx.Status}";
                var responseStream = webEx.Response?.GetResponseStream();
                if (responseStream != null)
                {
                    details += $", Reponse: {new StreamReader(responseStream).ReadToEnd()}";
                }
            }

            return details;
        }

        public static void WaitAndLog(this Task task, int seconds, string msg)
        {
            if (task == null)
            {
                Warning($"{System.Reflection.MethodBase.GetCurrentMethod().Name} extension method called on a null task");
                return;
            }

            try
            {
                if (!task.Wait(TimeSpan.FromSeconds(seconds)))
                {
                    Warning(msg);
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

        public static string Subject(this string tripleString)
        {
            return GetTripleObject(tripleString, TripleObjects.Subject);
        }

        public static string Predicate(this string tripleString)
        {
            return GetTripleObject(tripleString, TripleObjects.Predicate);
        }

        public static string Object(this string tripleString)
        {
            return GetTripleObject(tripleString, TripleObjects.Object);
        }

        private static string GetTripleObject(string tripleAsString, TripleObjects tripleObject)
        {
            var splitString = tripleAsString.Split(new[] { " , " }, StringSplitOptions.None);

            switch (tripleObject)
            {
                case TripleObjects.Subject:
                    return splitString[0];
                case TripleObjects.Predicate:
                    return splitString[1];
                case TripleObjects.Object:
                    return splitString[2];
                default:
                    throw new ArgumentOutOfRangeException(nameof(tripleObject), tripleObject, null);
            }
        }
    }
}
