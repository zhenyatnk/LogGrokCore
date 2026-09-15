using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using LogGrokX.Controls.TextRender;

namespace LogGrokX.Controls.ListControls
{
    public abstract class BaseListView : System.Windows.Controls.ListView
    {
        protected BaseListView()
        {
            CommandBindings.Add(new CommandBinding(RoutedCommands.CopyToClipboard,
                (_, args) => CopyToClipboard("CopyToClipboard", false, args),
                CanExecuteCopyToClipboard));
            CommandBindings.Add(new CommandBinding(RoutedCommands.CopyAsDisplayed,
                (_, args) => CopyToClipboard("CopyAsDisplayed", true, args),
                CanExecuteCopyToClipboard));
        }

        private void CopyToClipboard(string commandName, bool asDisplayed, ExecutedRoutedEventArgs args)
        {
            Trace.TraceInformation($"{commandName}.Execute");
            CopySelectedItemsToClipboard(asDisplayed);
            args.Handled = true;
        }

        private void CanExecuteCopyToClipboard(object _, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = GetSelectedIndices().Any();
            args.Handled = true;
        }
               
        private void CopySelectedItemsToClipboard(bool asDisplayed)
        {
            var indices = GetSelectedIndices();
            
            var items =  
                indices
                    .OrderBy(i => i)
                    .Select(i => Items[i]);
            
            var foldingState = asDisplayed ? TextView.GetSharedFoldingState(this) : null;
            var  text = new StringBuilder();
            foreach (var line in items)
            {
                var lineText = asDisplayed && line is BaseLogLineViewModel viewModel
                    ? viewModel.GetDisplayText(foldingState)
                    : line.ToString();
                _ = text.Append(lineText?.TrimEnd());
                _ = text.Append("\r\n");
            }
            _ = text.Replace("\0", string.Empty);

            TextCopy.ClipboardService.SetText(text.ToString());
        }

        protected abstract IEnumerable<int> GetSelectedIndices();
    }
}