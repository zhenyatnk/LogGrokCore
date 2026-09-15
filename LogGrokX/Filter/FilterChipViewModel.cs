using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LogGrokX.Data.Index;

namespace LogGrokX.Filter
{
    public class FilterChipViewModel : ViewModelBase
    {
        private readonly int _indexedComponentIndex;
        private readonly FilterSettings _filterSettings;

        public FilterChipViewModel(
            string fieldName,
            int indexedComponentIndex,
            IReadOnlyCollection<string> excludedValues,
            FilterSettings filterSettings)
        {
            FieldName = fieldName;
            _indexedComponentIndex = indexedComponentIndex;
            _filterSettings = filterSettings;

            DisplayText = excludedValues.Count == 1
                ? $"{fieldName}: {excludedValues.First()}"
                : $"{fieldName}: {excludedValues.Count} hidden";

            RemoveCommand = new DelegateCommand(
                () => _filterSettings.SetExclusions(_indexedComponentIndex, Enumerable.Empty<string>()));
        }

        public string FieldName { get; }

        public string DisplayText { get; }

        public ICommand RemoveCommand { get; }
    }
}
