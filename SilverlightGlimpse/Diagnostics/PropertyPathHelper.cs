using System.Windows;
using System.Windows.Data;

namespace SilverlightGlimpse.Diagnostics
{
    public static class PropertyPathHelper
    {
        private static readonly Dummy _dummy = new Dummy();

        public static object GetValue(object source, string propertyPath)
        {
            var binding = new Binding(propertyPath)
                              {
                                  Mode = BindingMode.OneTime, 
                                  Source = source
                              };
            BindingOperations.SetBinding(_dummy, Dummy.ValueProperty, binding);
            return _dummy.GetValue(Dummy.ValueProperty);
        }

        private class Dummy : DependencyObject
        {
            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", 
                                            typeof (object), 
                                            typeof (Dummy), 
                                            new PropertyMetadata(null));
        }
    }
}
