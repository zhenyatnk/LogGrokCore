using System;
using System.Collections.Generic;

namespace LogGrokCore.Data
{
    public sealed class TimeIndex
    {
        private readonly List<long> _ticks = new();
        private readonly List<int> _dayBoundaries = new();
        private long _lastTicks;
        private long _lastRawTicks;
        private long _lastDayNumber = long.MinValue;
        private long _dayOffset;
        private long _minTicks = long.MaxValue;
        private long _maxTicks = long.MinValue;

        public bool HasTime { get; private set; }

        public bool IsMonotonic { get; private set; } = true;

        public int Count => _ticks.Count;

        public IReadOnlyList<int> DayBoundaries => _dayBoundaries;

        public long MinTicks => _minTicks;

        public long MaxTicks => _maxTicks;

        public void Add(long ticks)
        {
            if (ticks < 0)
            {
                _ticks.Add(_lastTicks);
                return;
            }

            var normalized = ticks;
            if (ticks < TimeSpan.TicksPerDay)
            {
                if (_ticks.Count > 0 && ticks < _lastRawTicks &&
                    _lastRawTicks - ticks > TimeSpan.TicksPerHour)
                {
                    _dayOffset += TimeSpan.TicksPerDay;
                }

                normalized = ticks + _dayOffset;
                if (normalized < _lastTicks)
                    normalized = _lastTicks;
            }

            if (_ticks.Count > 0 && normalized < _lastTicks)
                IsMonotonic = false;

            _lastRawTicks = ticks;
            _lastTicks = normalized;
            HasTime = true;

            var dayNumber = normalized / TimeSpan.TicksPerDay;
            if (_ticks.Count > 0 && dayNumber != _lastDayNumber)
                _dayBoundaries.Add(_ticks.Count);
            _lastDayNumber = dayNumber;

            if (normalized < _minTicks) _minTicks = normalized;
            if (normalized > _maxTicks) _maxTicks = normalized;

            _ticks.Add(normalized);
        }

        public long GetTicksAt(int index) => _ticks[index];

        public (int StartLine, int EndLine)? FindLineRange(long fromTicks, long toTicks)
        {
            if (!HasTime || !IsMonotonic || _ticks.Count == 0)
                return null;

            var start = LowerBound(fromTicks);
            var end = UpperBound(toTicks);
            if (end < start) end = start;

            return (start, end);
        }

        private int LowerBound(long ticks)
        {
            var low = 0;
            var high = _ticks.Count;
            while (low < high)
            {
                var mid = low + (high - low) / 2;
                if (_ticks[mid] < ticks)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }

        private int UpperBound(long ticks)
        {
            var low = 0;
            var high = _ticks.Count;
            while (low < high)
            {
                var mid = low + (high - low) / 2;
                if (_ticks[mid] <= ticks)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }
    }
}
