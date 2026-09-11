using System.Text.RegularExpressions;
using System.Windows.Media;
using LogGrokCore.Colors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaColors = System.Windows.Media.Colors;

namespace LogGrokCore.Tests
{
    [TestClass]
    public class ColorSettingsTests
    {
        private static double Luminance(Color color) =>
            (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;

        private static Color GetColor(Brush brush)
        {
            Assert.IsNotNull(brush);
            return ((SolidColorBrush)brush).Color;
        }

        [TestMethod]
        public void LightThemeKeepsConfiguredColors()
        {
            var rule = new ColorSettings.ColorRule(new Regex("IMP"), MediaColors.DarkRed, MediaColors.White);

            Assert.AreEqual(MediaColors.DarkRed, GetColor(rule.GetForegroundBrush(false)));
            Assert.AreEqual(MediaColors.White, GetColor(rule.GetBackgroundBrush(false)));
        }

        [TestMethod]
        public void DarkThemeLightensDarkForeground()
        {
            var rule = new ColorSettings.ColorRule(new Regex("IMP"), MediaColors.DarkRed, null);

            var dark = GetColor(rule.GetForegroundBrush(true));

            Assert.IsTrue(Luminance(dark) > Luminance(MediaColors.DarkRed));
            Assert.IsTrue(Luminance(dark) >= 0.5);
        }

        [TestMethod]
        public void DarkThemeKeepsReadableForeground()
        {
            var rule = new ColorSettings.ColorRule(new Regex("DBG"), MediaColors.Gray, null);

            Assert.AreEqual(MediaColors.Gray, GetColor(rule.GetForegroundBrush(true)));
        }

        [TestMethod]
        public void DarkThemeDarkensLightBackground()
        {
            var rule = new ColorSettings.ColorRule(new Regex("fatal"), null, MediaColors.White);

            var dark = GetColor(rule.GetBackgroundBrush(true));

            Assert.IsTrue(Luminance(dark) < Luminance(MediaColors.White));
            Assert.IsTrue(Luminance(dark) <= 0.3);
        }
    }
}
