using System;
using static Serilog.Log;

namespace Common
{
    public class StaticMethods
    {
        public static T ConvertTo<T>(object value)
        {
            try
            {
                return (T) Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                Logger.Debug($"Failed to convert {value.GetType()} to {typeof(T)}: {e.GetDetails()}");
                throw;
            }
        }
    }
}
