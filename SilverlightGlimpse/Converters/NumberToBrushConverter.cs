using System;
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media;
using SilverlightGlimpse.Xaml;

namespace SilverlightGlimpse.Converters
{
    public class NumberToBrushConverter : MarkupExtension, IMultiValueConverter
    {
        private static NumberToBrushConverter _converter;
        private readonly SolidColorBrush INCORRECT_USAGE_OF_CONVERTER = new SolidColorBrush(Color.FromArgb(255, 255, 255,0));

        /// <summary>
        /// Returns a SolidBrushColour based on whether the count = 0 or not
        /// </summary>
        /// <param name="values">Array of objects: value[0] = Count, value[1] = ZeroBrush, value[2] = OverZeroBrush</param>
        /// <param name="targetType">Not used.</param>
        /// <param name="parameter">Not used.</param>
        /// <param name="culture">Not used.</param>
        /// <returns>SolidBrushColor</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 3) return INCORRECT_USAGE_OF_CONVERTER;

            int count; 
            if (!Int32.TryParse(values[0].ToString(), out count)) return INCORRECT_USAGE_OF_CONVERTER;

            var zeroBrush = values[1] as SolidColorBrush;
            if (zeroBrush == null) return INCORRECT_USAGE_OF_CONVERTER;

            var overZeroBrush = values[2] as SolidColorBrush;
            if (overZeroBrush == null) return INCORRECT_USAGE_OF_CONVERTER;
            
            return count == 0
                ? zeroBrush
                : overZeroBrush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new NumberToBrushConverter());
        }
    }
}
