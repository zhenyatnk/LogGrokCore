using System.Collections.Generic;

namespace LogGrokCore.Data
{
    public sealed class TimeIndex
    {
        private readonly List<long> _ticks = new();
        private long _lastTicks;
        private long _minTicks = long.MaxValue;
        private long _maxTicks = long.MinValue;

        public bool HasTime { get; private set; }

        public bool IsMonotonic { get; private set; } = true;

        public int Count => _ticks.Count;

        public long MinTicks => _minTicks;

        public long MaxTicks => _maxTicks;

        public void Add(long ticks)
        {
            if (ticks < 0)
            {
                _ticks.Add(_lastTicks);
                return;
            }

            if (_ticks.Count > 0 && ticks < _lastTicks)
                IsMonotonic = false;

            _lastTicks = ticks;
            HasTime = true;

            if (ticks < _minTicks) _minTicks = ticks;
            if (ticks > _maxTicks) _maxTicks = ticks;

            _ticks.Add(ticks);
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
