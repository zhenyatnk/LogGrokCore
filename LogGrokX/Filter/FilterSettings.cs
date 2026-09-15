using System;
using System.Collections.Generic;
using System.Linq;
using LogGrokX.Data;
using LogGrokX.Data.Index;

namespace LogGrokX.Filter
{
    public class FilterSettings
    {
        private readonly Dictionary<int, HashSet<string>> _exclusions = new();
        private readonly Indexer _indexer;

        public bool HaveExclusions => _exclusions.Values.Any(exclusions => exclusions.Count > 0);

        public (long From, long To)? TimeRange { get; private set; }

        public bool HasTimeRange => TimeRange != null;

        public (int From, int To)? LineRange { get; private set; }

        public bool HasLineRange => LineRange != null;
        
        public event Action? ExclusionsChanged;

        public event Action? TimeRangeChanged;

        public event Action? LineRangeChanged;

        public FilterSettings(Indexer indexer, LogMetaInformation metaInformation)
        {
            _indexer = indexer;
        }

        public IReadOnlyDictionary<int, IEnumerable<string>> Exclusions
        {
            get
            {
                return _exclusions
                    .Where(kv => kv.Value.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value as IEnumerable<string>);
            }
        }

        public bool this[(int component, string value) arg]
        {
            get
            {
                if (!_exclusions.TryGetValue(arg.component, out var componentExclusions))
                    return true;
                return !componentExclusions.Contains(arg.value);
            }
            set
            {
                var areExclusionsExist = _exclusions.TryGetValue(arg.component, out var componentExclusions);

                if (value && componentExclusions != null)
                {
                    componentExclusions.Remove(arg.value);
                    ExclusionsChanged?.Invoke();
                }
                else
                {
                    if (!areExclusionsExist)
                    {
                        componentExclusions = new HashSet<string>();
                        _exclusions[arg.component] = componentExclusions;
                    }

                    if (componentExclusions?.Add(arg.value) is true)
                    {
                        ExclusionsChanged?.Invoke();
                    }
                }
            }
        }

        public void AddExclusions(int indexedComponent, IEnumerable<string> componentValuesToExclude)
        {
            if (!_exclusions.TryGetValue(indexedComponent, out var currentExclusions))
            {
                currentExclusions = new HashSet<string>();
            }

            SetExclusions(indexedComponent, currentExclusions.Concat(componentValuesToExclude));
        }

        public void ExcludeAllExcept(int indexedComponent, IEnumerable<string> componentValuesToInclude)
        {
            var exclusions = _indexer.GetAllComponents(indexedComponent).Except(componentValuesToInclude);
            SetExclusions(indexedComponent , exclusions);
        }

        public void ClearAllExclusions()
        {
            _exclusions.Clear();
            ExclusionsChanged?.Invoke();
        }

        public void SetExclusions(int indexedComponent, IEnumerable<string> componentValuesToExclude)
        {
            _exclusions[indexedComponent] = componentValuesToExclude.ToHashSet();
            ExclusionsChanged?.Invoke();
        }

        public void SetTimeRange(long fromTicks, long toTicks)
        {
            if (TimeRange == (fromTicks, toTicks)) return;
            TimeRange = (fromTicks, toTicks);
            TimeRangeChanged?.Invoke();
        }

        public void ClearTimeRange()
        {
            if (TimeRange == null) return;
            TimeRange = null;
            TimeRangeChanged?.Invoke();
        }

        public void SetLineRange(int fromLine, int toLine)
        {
            if (LineRange == (fromLine, toLine)) return;
            LineRange = (fromLine, toLine);
            LineRangeChanged?.Invoke();
        }

        public void ClearLineRange()
        {
            if (LineRange == null) return;
            LineRange = null;
            LineRangeChanged?.Invoke();
        }
    }
}