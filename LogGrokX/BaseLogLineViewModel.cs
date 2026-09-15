using System;
using LogGrokX.Controls;
using LogGrokX.Controls.TextRender;
using LogGrokX.MarkedLines;

namespace LogGrokX
{
    public abstract class BaseLogLineViewModel : ItemViewModel, ILineMark
    {
        private readonly Selection _markedLines;

        protected BaseLogLineViewModel(int index, Selection markedLines)
        {
            Index = index;
            IndexViewModel = new LinePartViewModel(HashCode.Combine(-1, index), Index.ToString());
            _markedLines = markedLines;
            _markedLines.Changed += () => InvokePropertyChanged(nameof(IsMarked));
        }
        
        public int Index { get; }

        public LinePartViewModel IndexViewModel { get; }

        public virtual string GetDisplayText(TextViewSharedFoldingState? foldingState) =>
            ToString() ?? string.Empty;
        
        public bool IsMarked
        {
            get => _markedLines.Contains(Index);
            set
            {
                if (value)
                    _markedLines.Add(Index);
                else 
                    _markedLines.Remove(Index);
            }
        }
    }
}