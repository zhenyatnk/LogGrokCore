using System;
using System.Globalization;

namespace LogGrokX;

internal static class DurationFormatter
{
    public static string Format(long ticks)
    {
        if (ticks <= 0)
            return "0 ms";

        if (ticks < TimeSpan.TicksPerSecond)
        {
            var milliseconds = ticks / TimeSpan.TicksPerMillisecond;
            return milliseconds == 1 ? "1 ms" : $"{milliseconds} ms";
        }

        if (ticks < TimeSpan.TicksPerMinute)
        {
            var seconds = (double)ticks / TimeSpan.TicksPerSecond;
            return $"{seconds.ToString("0.#", CultureInfo.InvariantCulture)} sec";
        }

        if (ticks < TimeSpan.TicksPerHour)
        {
            var seconds = ticks / TimeSpan.TicksPerSecond;
            var minutes = seconds / 60;
            var remainder = seconds % 60;
            return remainder == 0 ? $"{minutes} min" : $"{minutes} min {remainder} sec";
        }

        if (ticks < TimeSpan.TicksPerDay)
        {
            var minutes = ticks / TimeSpan.TicksPerMinute;
            var hours = minutes / 60;
            var remainder = minutes % 60;
            return remainder == 0 ? $"{hours} h" : $"{hours} h {remainder} min";
        }

        var totalHours = ticks / TimeSpan.TicksPerHour;
        var days = totalHours / 24;
        var restHours = totalHours % 24;
        return restHours == 0 ? $"{days} d" : $"{days} d {restHours} h";
    }
}
