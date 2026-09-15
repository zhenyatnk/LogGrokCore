using System;
using System.Text;
using LogGrokX.Controls;
using LogGrokX.Controls.TextRender;
using LogGrokX.Data;

namespace LogGrokX
{
    public class LineViewModel : BaseLogLineViewModel
    {
        private readonly string _sourceString;

        private readonly ParseResult _parseResult;
        private readonly string _transformResult;

        public LineViewModel(int index, string sourceString, ILineParser parser, Selection markedLines,
            TransformationPerformer transformationPerformer)
            : base(index, markedLines)
        {
            _sourceString = sourceString;
            _transformResult = transformationPerformer.Transform(sourceString); 
            _parseResult = parser.Parse(
                _transformResult);
        }

        public LinePartViewModel this[int index] => GetValue(index);

        private LinePartViewModel GetValue(int index)
        {
            var uniqueId = HashCode.Combine(base.Index, index);
            var lineMeta = _parseResult.Get().ParsedLineComponents;
            var text = _transformResult.Substring(lineMeta.ComponentStart(index),
                lineMeta.ComponentLength(index));

            return new LinePartViewModel(uniqueId, text);
        }

        public override bool Equals(object? o)
        {
            if (o is LineViewModel other)
                return other.Index == Index && other._sourceString == _sourceString;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Index, _sourceString);
        }

        public override string ToString()
        {
            return _transformResult.TrimEnd();
        }

        public override string GetDisplayText(TextViewSharedFoldingState? foldingState)
        {
            var lineMeta = _parseResult.Get().ParsedLineComponents;
            var componentCount = _parseResult.ComponentCount;
            var builder = new StringBuilder();
            var offset = 0;

            for (var i = 0; i < componentCount; i++)
            {
                var start = lineMeta.ComponentStart(i);
                var length = lineMeta.ComponentLength(i);

                if (start < offset || start + length > _transformResult.Length)
                    continue;

                builder.Append(_transformResult, offset, start - offset);
                var componentText = _transformResult.Substring(start, length);
                builder.Append(GetComponentDisplayText(i, componentText, foldingState));
                offset = start + length;
            }

            builder.Append(_transformResult, offset, _transformResult.Length - offset);
            return builder.ToString().TrimEnd();
        }

        private string GetComponentDisplayText(int componentIndex, string componentText,
            TextViewSharedFoldingState? foldingState)
        {
            var textModel = new TextModel(HashCode.Combine(Index, componentIndex), componentText);

            if (foldingState == null || textModel.CollapsibleRanges == null)
                return textModel.GetDisplayedText(null);

            var collapsedLines = foldingState[textModel.UniqueId]
                                 ?? foldingState.GetDefaultSettings(textModel.CollapsibleRanges, textModel.Count);

            return textModel.GetDisplayedText(collapsedLines);
        }
    }
}
