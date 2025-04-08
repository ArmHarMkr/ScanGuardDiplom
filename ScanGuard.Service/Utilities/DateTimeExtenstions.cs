using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.BLL.Utilities
{
    public static class DateTimeExtensions
    {
        public static string ToRelativeTime(this DateTime dateTime)
        {
            var span = DateTime.UtcNow - dateTime.ToUniversalTime();

            if (span.TotalMinutes < 1)
                return "just now";
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} minutes ago";
            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} hours ago";
            return $"{(int)span.TotalDays} days ago";
        }
    }
}
