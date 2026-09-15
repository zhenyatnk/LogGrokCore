using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using LogGrokX.Colors;
using LogGrokX.Controls;
using LogGrokX.Controls.TextRender;
using LogGrokX.Data;
using LogGrokX.Search;

namespace LogGrokX
{
    public class DocumentViewModel : ViewModelBase
    {
        private readonly Selection _markedLines;
        private bool _isCurrentDocument;
        private readonly LineProvider _lineProvider;
        private readonly ILineParser _lineParser;
        private readonly TransformationPerformer _transformationPerformer;
        private Stream _fileHolder;

        public DocumentViewModel(
            LineProvider lineProvider,
            LogModelFacade logModelFacade,
            LogViewModel logViewModel, 
            SearchViewModel searchViewModel,
            Selection markedLines,
            ColorSettings colorSettings,
            TransformationPerformer transformationPerformer,
            TextViewSharedFoldingState foldingState)
        {
            var logFileFilePath = logModelFacade.LogFile.FilePath;
            
            Title = 
                Path.GetFileName(logFileFilePath) 
                    ?? throw new InvalidOperationException($"Invalid path: {logFileFilePath}");

            LogViewModel = logViewModel;
            SearchViewModel = searchViewModel;
            ColorSettings = colorSettings;
            FoldingState = foldingState;
            
            SearchViewModel.CurrentLineChanged += lineNumber => NavigateTo(lineNumber);
            SearchViewModel.CurrentSearchChanged += regex => LogViewModel.HighlightRegex = regex;
            SearchViewModel.PropertyChanged += OnSearchViewModelPropertyChanged;
            LogViewModel.SetSearchMatches(SearchViewModel.CurrentMatchBuckets);
            LogViewModel.SetSearchMatchLine(SearchViewModel.CurrentMatchLine);

            _markedLines = markedLines;
            _transformationPerformer = transformationPerformer;
            _lineProvider = lineProvider;
            _lineParser = logModelFacade.LineParser;
            _markedLines.Changed += () => MarkedLinesChanged?.Invoke();
            _fileHolder = logModelFacade.LogFile.Open();

            CopyPathToClipboardCommand =
                new DelegateCommand(() => TextCopy.ClipboardService.SetText(logFileFilePath));

            CopyFilenameToClipboardCommand = new DelegateCommand(() => TextCopy.ClipboardService.SetText(Path.GetFileName(logFileFilePath)));

            OpenContainingFolderCommand = new DelegateCommand(() => OpenContainingFolder(logFileFilePath));
            FindNextCommand = new DelegateCommand(() => SearchViewModel.FindNext(GetSearchAnchor()));
            FindPreviousCommand = new DelegateCommand(() => SearchViewModel.FindPrevious(GetSearchAnchor()));
            DocumentId = logFileFilePath;
        }

        private int GetSearchAnchor()
        {
            var selectionLine = LogViewModel.CurrentOriginalLine;
            if (selectionLine >= 0)
                return selectionLine;

            return SearchViewModel.CurrentMatchLine;
        }

        public string DocumentId { get; }
    
        public event Action? MarkedLinesChanged;
        
        public ICommand CopyPathToClipboardCommand { get; }

        public ICommand CopyFilenameToClipboardCommand { get; }

        public ICommand OpenContainingFolderCommand { get; }

        public ICommand FindNextCommand { get; }

        public ICommand FindPreviousCommand { get; }

        public void NavigateTo(int lineNumber)
        {
            LogViewModel.NavigateTo(lineNumber);
        }

        private void OnSearchViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchViewModel.CurrentMatchBuckets))
                LogViewModel.SetSearchMatches(SearchViewModel.CurrentMatchBuckets);

            if (e.PropertyName == nameof(SearchViewModel.CurrentMatchLine))
                LogViewModel.SetSearchMatchLine(SearchViewModel.CurrentMatchLine);
        }
        
        public ObservableCollection<(int number, string text)> MarkedLineViewModels
        {
            get
            {
                var lineNumbers = _markedLines.ToList();
                lineNumbers.Sort();
                var collection = new ObservableCollection<(int number, string text)>();
                foreach (var lineNumber in lineNumbers)
                {
                    var lines = new (int, string)[1];
                    _lineProvider.Fetch(lineNumber, lines.AsSpan());
                    collection.Add((lineNumber, _transformationPerformer.Transform(lines[0].Item2)));
                }

                return collection;
            }
        }

        public Selection MarkedLines => _markedLines;        
        
        public string Title { get; }

        public LogViewModel LogViewModel { get; }

        public SearchViewModel SearchViewModel { get; }

        public ColorSettings ColorSettings { get; }

        public TextViewSharedFoldingState FoldingState { get; }

        public int GetFoldingComponentIndex(string transformedText)
        {
            var parseResult = _lineParser.Parse(transformedText);
            var lineMeta = parseResult.Get().ParsedLineComponents;
            for (var i = 0; i < parseResult.ComponentCount; i++)
            {
                var length = lineMeta.ComponentLength(i);
                if (length <= 0)
                    continue;

                var componentText = transformedText.Substring(lineMeta.ComponentStart(i), length);
                if (TextOperations.GetJsonRanges(componentText).Any())
                    return i;
            }

            return 0;
        }

        public bool IsCurrentDocument
        {
            get => _isCurrentDocument;
            set => SetAndRaiseIfChanged(ref _isCurrentDocument, value);
        }
        
        private void OpenContainingFolder(string path)
        {
            var filePath = path;

            var cmdLine = File.Exists(filePath)
                ? $"/select, {filePath}"
                : $"/select, {Directory.GetParent(filePath)?.FullName}";

            _ = Process.Start("explorer.exe", cmdLine);
        }

        public void CloseFile()
        {
            _fileHolder.Dispose();
        }
    }
}