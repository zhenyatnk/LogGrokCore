using System;

namespace LogGrokX.Data.Index
{
    public sealed class LineRangeIndexedLinesProvider : IIndexedLinesProvider
    {
        private readonly IIndexedLinesProvider _source;
        private readonly int _startPosition;
        private readonly int _endPosition;

        public LineRangeIndexedLinesProvider(IIndexedLinesProvider source, int startLine, int endLine)
        {
            _source = source;
            _startPosition = FindLowerBound(startLine);
            _endPosition = Math.Max(_startPosition, FindLowerBound(endLine));
        }

        public int Count => _endPosition - _startPosition;

        public void Fetch(int start, Span<int> values)
        {
            _source.Fetch(_startPosition + start, values);
        }

        public int GetIndexByValue(int value)
        {
            var position = FindLowerBound(value);
            return Math.Clamp(position - _startPosition, 0, Count);
        }

        private int FindLowerBound(int line)
        {
            var low = 0;
            var high = _source.Count;
            Span<int> buffer = stackalloc int[1];
            while (low < high)
            {
                var mid = low + (high - low) / 2;
                _source.Fetch(mid, buffer);
                if (buffer[0] < line)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }
    }
}
