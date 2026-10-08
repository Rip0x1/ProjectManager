using System.Globalization;
using System.Windows.Data;

namespace ProjectManagementSystem.WPF.Converters
{
    public class PageNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int page && page > 0)
            {
                return page.ToString(culture);
            }

            return "1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string text || string.IsNullOrWhiteSpace(text))
            {
                return 1;
            }

            var digits = new string(text.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(digits))
            {
                return 1;
            }

            if (int.TryParse(digits, out var page) && page >= 1)
            {
                return page;
            }

            return 1;
        }
    }
}
