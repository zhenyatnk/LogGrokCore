﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using LogGrokCore.AvalonDockExtensions;

namespace LogGrokCore
{
    public static class Constants
    {
        public const string MarkedLinesContentId = "###MarkedLinesContentId";

        public const string DefaultAvalonDockLayout =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
                <LayoutRoot>
                    <RootPanel Orientation=""Horizontal"">
                    <LayoutDocumentPane />
                    <LayoutAnchorablePane DockWidth=""300"" DocMinWidth=""300"">
                    <LayoutAnchorable 
                        AutoHideMinWidth=""100"" 
                        AutoHideMinHeight=""100"" 
                        Title=""Marked lines"" 
                        IsSelected=""True"" 
                        ContentId=""###MarkedLinesContentId"" 
                        CanClose=""False"" 
                        CanHide=""False""/>
                    </LayoutAnchorablePane>
                    </RootPanel>
                    <TopSide />
                    <RightSide />
                    <LeftSide />
                    <BottomSide />
                    <FloatingWindows />
                    <Hidden />
        </LayoutRoot>";
    }

    public partial class MainWindow
    {
        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            var operatingSystem = Environment.OSVersion;
            if (operatingSystem.Version.Build >= 22000) // windows 11 heuristic
            {
                Trace.TraceInformation("Windows 11 detected");
                WindowChrome.SetWindowChrome(this, new WindowChrome() { CaptionHeight = 0 });
                WindowStyle = WindowStyle.None;
            }
            
            DataContext = mainWindowViewModel;
            Closing += SaveLayout;
            Loaded += LoadLayout;
            
            InitializeComponent();
        }

        private void LoadLayout(object sender, RoutedEventArgs e)
        {
            var settingsFileName = GetSettingsFileName();
            using var reader =
                File.Exists(settingsFileName)
                    ? new StreamReader(settingsFileName)
                    : new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(Constants.DefaultAvalonDockLayout)));
            
            var contentProvider = (IContentProvider) DataContext;
            var layoutSerializer = new XmlLayoutSerializer(DockingManager);
            
            layoutSerializer.LayoutSerializationCallback += (_, args) =>
            {
                if (args.Model is LayoutDocument)
                {
                    args.Cancel = true;
                    return;
                }

                var content = contentProvider.GetContent(args.Model.ContentId);
                if (content != null)
                {
                    args.Content = new ContentControl { Content = content };
                }
            };
            layoutSerializer.Deserialize(reader);

            foreach (var layoutAnchorable in DockingManager.Layout.Children
                .OfType<LayoutAnchorable>()
                .Where(l => l.IsHidden).ToList())
            {
                layoutAnchorable.Show();
            }
        }

        private void SaveLayout(object? sender, CancelEventArgs e)
        {
            var fileName = GetSettingsFileName();
            var serializer = new XmlLayoutSerializer(DockingManager);
            using var writer = new StreamWriter(fileName);
            
            serializer.Serialize(writer);
        }
        private static string GetSettingsFileName()
        {
            return HomeDirectoryPathProvider.GetDataFileFullPath("layout.settings");
        }
    }
}