using System;
using LogGrokCore.Colors;
using LogGrokCore.Controls.TextRender;

namespace LogGrokCore.MarkedLines
{
    public class MarkedLineViewModel : BaseLogLineViewModel
    {
        public DocumentViewModel Document { get; }
        public LinePartViewModel Text { get; }
        public ColorSettings ColorSettings { get; }

        public MarkedLineViewModel(DocumentViewModel document, int lineNumber, string text)
            : base(lineNumber, document.MarkedLines)        
        {
            Document = document;

            var uniqueId = HashCode.Combine(document, lineNumber);
            Text = new LinePartViewModel(uniqueId, text);
            ColorSettings = document.ColorSettings;
        }

        public override string ToString() => Text.OriginalText ?? string.Empty;

        public override string GetDisplayText(TextViewSharedFoldingState? foldingState)
        {
            var textModel = Text.TextModel;
            if (foldingState == null || textModel.CollapsibleRanges == null)
                return textModel.GetDisplayedText(null);

            var collapsedLines = foldingState[textModel.UniqueId]
                                 ?? foldingState.GetDefaultSettings(textModel.CollapsibleRanges, textModel.Count);

            return textModel.GetDisplayedText(collapsedLines);
        }
    }
}