using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectManagementSystem.WPF.Converters
{
    public class EditModeToHintConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditMode && parameter is string hints)
            {
                var hintArray = hints.Split('|');
                if (hintArray.Length == 2)
                {
                    return isEditMode ? hintArray[0] : hintArray[1];
                }
            }
            return "Введите пароль";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
