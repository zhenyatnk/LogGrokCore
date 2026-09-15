using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace LogGrokX.Controls
{
    public class FormatTextExtension : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && parameter is string format)
                return string.Format(format, text);
            return value ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
