using System;
using System.Globalization;
using System.Windows.Input;
using LogGrokCore.Data;

namespace LogGrokCore.Filter
{
    public sealed class TimeRangeFilterViewModel : ViewModelBase
    {
        private readonly TimeIndex _timeIndex;
        private readonly FilterSettings _filterSettings;
        private bool _suppress;
        private bool _isAvailable;
        private double _minimum;
        private double _maximum;
        private double _lowerValue;
        private double _upperValue;

        public TimeRangeFilterViewModel(TimeIndex timeIndex, FilterSettings filterSettings)
        {
            _timeIndex = timeIndex;
            _filterSettings = filterSettings;
            ResetCommand = new DelegateCommand(Reset);
        }

        public bool IsAvailable
        {
            get => _isAvailable;
            private set => SetAndRaiseIfChanged(ref _isAvailable, value);
        }

        public double Minimum
        {
            get => _minimum;
            private set => SetAndRaiseIfChanged(ref _minimum, value);
        }

        public double Maximum
        {
            get => _maximum;
            private set => SetAndRaiseIfChanged(ref _maximum, value);
        }

        public double LowerValue
        {
            get => _lowerValue;
            set
            {
                if (Equals(_lowerValue, value)) return;
                _lowerValue = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SelectedRangeText));
                InvokePropertyChanged(nameof(DurationText));
                Apply();
            }
        }

        public double UpperValue
        {
            get => _upperValue;
            set
            {
                if (Equals(_upperValue, value)) return;
                _upperValue = value;
                InvokePropertyChanged();
                InvokePropertyChanged(nameof(SelectedRangeText));
                InvokePropertyChanged(nameof(DurationText));
                Apply();
            }
        }

        public string MinText => TimestampParser.Format((long)_minimum);

        public string MaxText => TimestampParser.Format((long)_maximum);

        public string SelectedRangeText
        {
            get
            {
                var from = TimestampParser.Format((long)_lowerValue);
                var to = TimestampParser.Format((long)_upperValue);
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                    return string.Empty;

                var duration = TimeSpan.FromTicks(Math.Max(0, (long)_upperValue - (long)_lowerValue));
                return $"{from} - {to} ({FormatDuration(duration)})";
            }
        }

        public string DurationText
        {
            get
            {
                if (!IsAvailable)
                    return string.Empty;

                var duration = TimeSpan.FromTicks(Math.Max(0, (long)_upperValue - (long)_lowerValue));
                return FormatDurationCompact(duration);
            }
        }

        public ICommand ResetCommand { get; }

        public void Refresh()
        {
            var available = _timeIndex.HasTime && _timeIndex.IsMonotonic &&
                            _timeIndex.Count > 0 && _timeIndex.MaxTicks > _timeIndex.MinTicks;

            _suppress = true;
            try
            {
                Minimum = _timeIndex.MinTicks;
                Maximum = _timeIndex.MaxTicks;
                LowerValue = Minimum;
                UpperValue = Maximum;
            }
            finally
            {
                _suppress = false;
            }

            InvokePropertyChanged(nameof(MinText));
            InvokePropertyChanged(nameof(MaxText));
            InvokePropertyChanged(nameof(SelectedRangeText));
            IsAvailable = available;
            InvokePropertyChanged(nameof(DurationText));
        }

        private void Apply()
        {
            if (_suppress || !IsAvailable) return;

            var min = _timeIndex.MinTicks;
            var max = _timeIndex.MaxTicks;
            var lower = Math.Clamp((long)_lowerValue, min, max);
            var upper = Math.Clamp((long)_upperValue, min, max);

            if (lower < upper && (lower > min || upper < max))
                _filterSettings.SetTimeRange(lower, upper);
            else
                _filterSettings.ClearTimeRange();
        }

        public void Reset()
        {
            _suppress = true;
            try
            {
                LowerValue = Minimum;
                UpperValue = Maximum;
            }
            finally
            {
                _suppress = false;
            }

            InvokePropertyChanged(nameof(SelectedRangeText));
            InvokePropertyChanged(nameof(DurationText));
            _filterSettings.ClearTimeRange();
        }

        private static string FormatDuration(TimeSpan duration) =>
            duration.TotalHours >= 1
                ? duration.ToString(@"h\:mm\:ss\.fff", CultureInfo.InvariantCulture)
                : duration.ToString(@"m\:ss\.fff", CultureInfo.InvariantCulture);

        private static string FormatDurationCompact(TimeSpan duration) =>
            duration.TotalHours >= 1
                ? $"{(int)duration.TotalHours}:{duration.Minutes:00}:{duration.Seconds:00}"
                : $"{duration.Minutes}:{duration.Seconds:00}";
    }
}
