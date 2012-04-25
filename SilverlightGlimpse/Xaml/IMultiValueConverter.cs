using System;
using System.Globalization;

namespace SilverlightGlimpse.Xaml
{
    public interface IMultiValueConverter
    {
        Object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
        Object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
    }
}