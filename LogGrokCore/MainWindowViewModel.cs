using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Input;
using LogGrokCore.AvalonDockExtensions;
using LogGrokCore.Data;
using LogGrokCore.MarkedLines;
using LogGrokCore.Search;
using LogGrokCore.Theming;
using Microsoft.Win32;

namespace LogGrokCore
{
    public class MainWindowViewModel : ViewModelBase, IContentProvider, IDisposable
    {
        private DocumentViewModel? _currentDocument;
        private readonly ApplicationSettings _applicationSettings;
        private readonly SearchAutocompleteCache _searchAutocompleteCache;
        private readonly SavedSearchPatternStore _savedSearchPatternStore;
        private readonly UiThemeService _themeService;

        public ObservableCollection<DocumentViewModel> Documents { get; }

        public MarkedLinesViewModel MarkedLinesViewModel { get; }
        
        public ICommand OpenFileCommand => new DelegateCommand(OpenFile);

        public ICommand OnDocumentCloseCommand => DelegateCommand.Create((DocumentViewModel document) =>
        {
            document.CloseFile();
        });

        public ICommand DropCommand => new DelegateCommand(
            obj=> OpenFiles((IEnumerable<string>)obj), 
            o => o is IEnumerable<string>);

        public MainWindowViewModel(ApplicationSettings applicationSettings, 
            SearchAutocompleteCache searchAutocompleteCache, 
            SavedSearchPatternStore savedSearchPatternStore,
            UiThemeService themeService,
            Func<ObservableCollection<DocumentViewModel>, MarkedLinesViewModel> markedLinesViewModelFactory)
        {
            _applicationSettings = applicationSettings;
            _searchAutocompleteCache = searchAutocompleteCache;
            _savedSearchPatternStore = savedSearchPatternStore;
            _themeService = themeService;
            Documents = new ObservableCollection<DocumentViewModel>();
            MarkedLinesViewModel = markedLinesViewModelFactory(Documents);
            OpenSettings = new DelegateCommand(() =>
            { 
                OpenExternalFile(ApplicationSettings.SettingsFileName);
            });
            ToggleThemeCommand = new DelegateCommand(ToggleTheme);

            MarkedLinesViewModel.NavigationRequested += (document, index) =>
            {
                CurrentDocument = document;
                document.NavigateTo(index);
            };
        }

        private static void OpenExternalFile(string fileName)
        {
            void StartProcess(string verb)
            {
                using var process = new Process
                {
                    StartInfo =
                    {
                        FileName = fileName,
                        UseShellExecute = true,
                        Verb = verb
                    }
                };
                process.Start();
            }

            try
            {
                StartProcess(string.Empty);
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == 1155) // 'No application is associated with the specified file for this operation.'
                {
                    StartProcess("openas");
                }
                else throw;
            }
        }

        public DocumentViewModel? CurrentDocument
        {
            get => _currentDocument;
            set
            {
                if (_currentDocument == value) return;
                
                if (_currentDocument != null)
                    _currentDocument.IsCurrentDocument = false;
                
                _currentDocument = value;
                
                if (_currentDocument != null)
                    _currentDocument.IsCurrentDocument = true;
                
                InvokePropertyChanged();
            }
        }

        public ICommand OpenSettings { get; }

        public ICommand ToggleThemeCommand { get; }

        public bool IsDarkTheme => _themeService.IsDark;

        private static readonly AvalonDock.Themes.MetroTheme LightDockTheme = new();
        private static readonly AvalonDock.Themes.Vs2013DarkTheme DarkDockTheme = new();

        public AvalonDock.Themes.Theme DockTheme => _themeService.IsDark ? DarkDockTheme : LightDockTheme;

        private void ToggleTheme()
        {
            _themeService.Toggle();
            InvokePropertyChanged(nameof(IsDarkTheme));
            InvokePropertyChanged(nameof(DockTheme));
        }

        public ICommand ExitCommand => new DelegateCommand(() => Application.Current.Shutdown());

        private void OpenFile()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = "log",
                Filter = "All Files|*.*|Log files(*.log)|*.log|Text files(*.txt)|*.txt",
                Multiselect = true
            };

            var dialogResult = dialog.ShowDialog();
            if (dialogResult.GetValueOrDefault())
            {
                foreach (var fileName in dialog.FileNames)
                {
                    Trace.TraceInformation($"Open document {fileName}.");
                    AddDocument(fileName);
                }
            }
            
            ShowScratchPad?.Invoke(this, new EventArgs());
        }
        
        private void OpenFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                AddDocument(file);
            }
        }

        public void AddDocument(string fileName)
        {
            if (File.Exists(fileName))
                CurrentDocument = CreateDocument(fileName);
            else
                Trace.TraceError($"File {fileName} is not exists");
        }

        private DocumentViewModel CreateDocument(string fileName)
        {
            var container = new DocumentContainer(fileName, _applicationSettings, _searchAutocompleteCache, _savedSearchPatternStore);
            var viewModel = container.GetDocumentViewModel();
            Documents.Add(viewModel);
            Documents.CollectionChanged += (o, e) =>
            {
                if (Documents.Contains(viewModel))
                    return;
                container.Dispose();
            };
            return viewModel;
        }

        public event EventHandler? ShowScratchPad;
        
        public object? GetContent(string contentId)
        {
            return contentId == Constants.MarkedLinesContentId ? MarkedLinesViewModel : null;
        }

        public void Dispose()
        {
            Documents.Clear();
        }
    }
}
