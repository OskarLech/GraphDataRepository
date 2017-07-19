using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace QualityGrapher.Converters
{
    public class CommitPointTupleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var commitInfoList = value as IEnumerable<(ulong Id, DateTime CommitTime)>;
            return commitInfoList?.Select(tuple => $"Id: {tuple.Id}, Date: {tuple.CommitTime}").ToList();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var commitInfo = (string) value;
            var id = int.Parse(commitInfo.Substring(commitInfo.IndexOf(" ", StringComparison.Ordinal) + 1, 1));
            var substring = commitInfo.Substring(commitInfo.IndexOf("Date: ", StringComparison.Ordinal) + "Date: ".Length);
            var date = DateTime.Parse(substring);

            return ((ulong) id, date);
        }
    }
}
