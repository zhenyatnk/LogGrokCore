using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using ControlzEx.Theming;

namespace LogGrokCore.Theming
{
    public class UiThemeService
    {
        public const string LightTheme = "Light.Blue";
        public const string DarkTheme = "Dark.Blue";

        private readonly string _storeFileName =
            HomeDirectoryPathProvider.GetDataFileFullPath("Theme.json");

        public UiThemeService()
        {
            CurrentTheme = Load();
        }

        public string CurrentTheme { get; private set; }

        public bool IsDark => CurrentTheme.StartsWith("Dark", StringComparison.OrdinalIgnoreCase);

        public void ApplySavedTheme() => Apply(CurrentTheme);

        public void Apply(string themeName)
        {
            try
            {
                if (ThemeManager.Current.GetTheme(themeName, false) == null)
                    themeName = LightTheme;

                ThemeManager.Current.ChangeTheme(Application.Current, themeName, false);
                CurrentTheme = themeName;
                Save();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string Toggle()
        {
            Apply(IsDark ? LightTheme : DarkTheme);
            return CurrentTheme;
        }

        private void Save()
        {
            try
            {
                using var createStream = File.Create(_storeFileName);
                var options = new JsonSerializerOptions { WriteIndented = true };
                JsonSerializer.Serialize(createStream, CurrentTheme, options);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private string Load()
        {
            try
            {
                if (!File.Exists(_storeFileName)) return LightTheme;
                using var stream = File.OpenRead(_storeFileName);
                var themeName = JsonSerializer.Deserialize<string>(stream);
                return string.IsNullOrEmpty(themeName) ? LightTheme : themeName;
            }
            catch (Exception)
            {
                return LightTheme;
            }
        }
    }
}
