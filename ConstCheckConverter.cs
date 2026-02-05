using System;
using System.Globalization;
using System.Windows.Data;

namespace BPO_ex4
{
    // Конвертер, который проверяет, начинается ли ID с "CONST"
    public class ConstCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string id)
            {
                // Возвращает True, если ID начинается с CONST
                return id.StartsWith("CONST", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}