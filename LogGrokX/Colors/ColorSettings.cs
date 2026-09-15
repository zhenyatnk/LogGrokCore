using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace LogGrokX.Colors
{
    public class ColorSettings
    {
        public static readonly DependencyProperty ColorSettingsProperty = DependencyProperty.RegisterAttached(
            "ColorSettings",
            typeof(ColorSettings),
            typeof(ColorSettings),
            new FrameworkPropertyMetadata(default(ColorSettings), FrameworkPropertyMetadataOptions.Inherits));

        public static void SetColorSettings(DependencyObject element, ColorSettings value)
        {
            element.SetValue(ColorSettingsProperty, value);
        }

        public static ColorSettings? GetColorSettings(DependencyObject element)
        {
            return element.GetValue(ColorSettingsProperty) as ColorSettings;
        }

        public class ColorRule
        {
            public ColorRule(Regex regex, Color? foreground, Color? background)
            {
                Regex = regex;
                ForegroundColor = foreground;
                BackgroundColor = background;
            }

            public Color? ForegroundColor { get; }
            public Color? BackgroundColor { get; }
            public Regex Regex { get; }

            public bool IsMatch(string text)
            {
                return Regex.IsMatch(text);
            }

            public Brush? GetForegroundBrush(bool isDark) => GetBrush(ForegroundColor, isDark, false);

            public Brush? GetBackgroundBrush(bool isDark) => GetBrush(BackgroundColor, isDark, true);
        }

        public IReadOnlyList<ColorRule> Rules { get; }

        private static readonly ConcurrentDictionary<(Color, bool, bool), Brush> CachedBrushes = new();
        private static readonly ConcurrentDictionary<string, Regex> CachedRegexes = new();

        private static Brush? GetBrush(Color? color, bool isDark, bool isBackground)
        {
            if (color == null) return null;
            return CachedBrushes.GetOrAdd((color.Value, isDark, isBackground),
                key => CreateBrush(key.Item1, key.Item2, key.Item3));
        }

        private static Brush CreateBrush(Color color, bool isDark, bool isBackground)
        {
            var brush = new SolidColorBrush(isDark ? AdjustForDarkTheme(color, isBackground) : color);
            brush.Freeze();
            return brush;
        }

        private static Color AdjustForDarkTheme(Color color, bool isBackground)
        {
            if (color.A == 0) return color;

            var luminance = (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;

            if (isBackground)
            {
                const double target = 0.25;
                if (luminance <= target) return color;
                var scale = target / luminance;
                return Color.FromArgb(color.A,
                    (byte)Math.Round(color.R * scale),
                    (byte)Math.Round(color.G * scale),
                    (byte)Math.Round(color.B * scale));
            }

            const double targetLuminance = 0.5;
            if (luminance >= targetLuminance) return color;
            var factor = (targetLuminance - luminance) / (1.0 - luminance);
            byte Blend(byte c) => (byte)Math.Round(c + (255 - c) * factor);
            return Color.FromArgb(color.A, Blend(color.R), Blend(color.G), Blend(color.B));
        }

        public ColorSettings(Configuration.ColorSettings colorSettingsConfiguration)
        {
            Color? ParseColor(string colorString)
            {
                if (string.IsNullOrEmpty(colorString)) return null;
                return (Color)ColorConverter.ConvertFromString(colorString);
            }

            ColorRule Convert(Configuration.ColorRule rule)
            {
                return new ColorRule(
                    CachedRegexes.GetOrAdd(rule.RegexString, s => new Regex(s, RegexOptions.Compiled)),
                    ParseColor(rule.ForegroundColor),
                    ParseColor(rule.BackgroundColor));
            }

            Rules = colorSettingsConfiguration.Rules.Select(Convert).ToList();
        }
    }
}