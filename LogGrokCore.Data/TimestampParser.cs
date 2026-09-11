using System;
using System.Globalization;

namespace LogGrokCore.Data
{
    public static class TimestampParser
    {
        public static bool TryGetTicks(ReadOnlySpan<char> text, string? format, out long ticks)
        {
            ticks = -1;
            if (text.IsEmpty)
                return false;

            var hasFormat = !string.IsNullOrWhiteSpace(format);
            DateTime timestamp;
            var parsed = hasFormat
                ? DateTime.TryParseExact(text, format!, CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out timestamp)
                : DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestamp);

            if (!parsed)
                return false;

            ticks = hasFormat && IsTimeOnly(format!)
                ? timestamp.TimeOfDay.Ticks
                : timestamp.Ticks;

            return true;
        }

        public static string Format(long ticks)
        {
            if (ticks < 0)
                return string.Empty;

            var timestamp = new DateTime(ticks);
            return timestamp.Year <= 1
                ? timestamp.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture)
                : timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        }

        private static bool IsTimeOnly(string format) =>
            format.IndexOf('y') < 0 && format.IndexOf('M') < 0 && format.IndexOf('d') < 0;
    }
}
