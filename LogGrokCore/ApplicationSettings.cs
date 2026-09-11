using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LogGrokCore.Colors.Configuration;
using LogGrokCore.Controls.ListControls;
using LogGrokCore.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace LogGrokCore
{
    public class ApplicationSettings
    {
        private static ApplicationSettings? instance;

        public static string SettingsFileName => PathHelpers.GetLocalFilePath("appsettings.yaml");

        public DebugSettings DebugSettings { get; set; } = new();

        public ColorSettings ColorSettings { get; private set; } = new();

        public ViewSettings ViewSettings { get; private set; } = new();

        public LogFormat[] LogFormats { get; set; } =
            Array.Empty<LogFormat>();

        private readonly Dictionary<string, ColumnSettings> _columnSettingsMap = new();
        public ColumnSettings GetColumnSettings(string logFormat)
        {
            if (!_columnSettingsMap.TryGetValue(logFormat, out var columnSettings))
            {
                columnSettings = new ColumnSettings();
                _columnSettingsMap[logFormat] = columnSettings;
            }

            return columnSettings;
        }

        public void SetTimelineAtTop(bool isAtTop)
        {
            if (ViewSettings.TimelineAtTop == isAtTop)
                return;

            ViewSettings.TimelineAtTop = isAtTop;
            SaveTimelineAtTop(isAtTop);
        }

        private static void SaveTimelineAtTop(bool isAtTop)
        {
            try
            {
                if (!File.Exists(SettingsFileName))
                    return;

                var lines = File.ReadAllLines(SettingsFileName).ToList();
                var value = isAtTop ? "true" : "false";
                var keyRegex = new Regex(@"^(\s*)TimelineAtTop\s*:.*$");
                for (var i = 0; i < lines.Count; i++)
                {
                    var match = keyRegex.Match(lines[i]);
                    if (!match.Success) continue;
                    lines[i] = $"{match.Groups[1].Value}TimelineAtTop: {value}";
                    File.WriteAllLines(SettingsFileName, lines);
                    return;
                }

                var sectionRegex = new Regex(@"^(\s*)ViewSettings\s*:\s*$");
                for (var i = 0; i < lines.Count; i++)
                {
                    var match = sectionRegex.Match(lines[i]);
                    if (!match.Success) continue;
                    lines.Insert(i + 1, $"{match.Groups[1].Value}  TimelineAtTop: {value}");
                    File.WriteAllLines(SettingsFileName, lines);
                    return;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static ApplicationSettings Instance()
        {
            if (instance == null)
                instance = Load();
            return instance;
        }

        private static ApplicationSettings Load()
        {
            var builder = new ConfigurationBuilder()
                .AddYamlFile(SettingsFileName, true, true);

            var settings = new ApplicationSettings();

            var configuration = builder.Build();
            configuration.GetSection("Settings").Bind(settings);

            ChangeToken.OnChange(() => configuration.GetReloadToken(), () =>
            {
                var newSettings = new ApplicationSettings();
                configuration.GetSection("Settings").Bind(newSettings);
                settings.ColorSettings = newSettings.ColorSettings;
                settings.LogFormats = newSettings.LogFormats;
            });

            return settings;
        }

        private ApplicationSettings()
        {
        }
    }
}