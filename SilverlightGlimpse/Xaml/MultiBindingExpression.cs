﻿// Original code:  http://www.codeproject.com/Articles/286171/MultiBinding-in-Silverlight-5
// Author:  Henrik Jonsson

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace SilverlightGlimpse.Xaml
{
    public class MultiBindingExpression : INotifyPropertyChanged, IValueConverter, INotifyDataErrorInfo
    {
        private static readonly List<DependencyProperty> dataProperties = new List<DependencyProperty>();

        private static readonly DependencyProperty MultiBindingExpressionsProperty =
            DependencyProperty.RegisterAttached("MultiBindingExpressions", typeof (List<MultiBindingExpression>),
                                                typeof (MultiBinding), new PropertyMetadata(null));

        private int m_converterIndex = -1;
        private int m_converterParameterIndex = -1;
        private Object m_sourceValues;
        private int m_stringFormatIndex = -1;

        public MultiBindingExpression(DependencyObject target, MultiBinding multiBinding)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (multiBinding == null) throw new ArgumentNullException("multiBinding");

            Target = target;
            MultiBinding = multiBinding;
            ApplyToTarget();
        }

        public DependencyObject Target { get; private set; }

        public MultiBinding MultiBinding { get; private set; }

        private int m_sourceStartIndex { get; set; }
        private int m_sourceCount { get; set; }
        private int m_lastIndex { get; set; }

        private Action m_pendingUpdateAction { get; set; }

        public Object SourceValues
        {
            get { return m_sourceValues; }
            set
            {
                m_sourceValues = value;
                if (m_pendingUpdateAction == null)
                {
                    ProcessNewResult((Object[]) value);
                }
                OnPropertyChanged("SourceValues");
            }
        }

        public object Converter
        {
            get
            {
                if (m_converterIndex >= 0)
                {
                    return Target.GetValue(dataProperties[m_converterIndex]);
                }
                else
                {
                    return MultiBinding.Converter;
                }
            }
        }

        public object ConverterParameter
        {
            get
            {
                if (m_converterParameterIndex >= 0)
                {
                    return Target.GetValue(dataProperties[m_converterParameterIndex]);
                }
                else
                {
                    return MultiBinding.ConverterParameter;
                }
            }
        }

        public String StringFormat
        {
            get
            {
                if (m_stringFormatIndex >= 0)
                {
                    return Target.GetValue(dataProperties[m_stringFormatIndex]) as String;
                }
                else
                {
                    return MultiBinding.StringFormat;
                }
            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region IValueConverter implementation

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return DependencyProperty.UnsetValue;
            var unconvertedValues = (object[]) value;
            object converter = Converter;

            object result = unconvertedValues;
            if (converter != null)
            {
                object converterParameter = ConverterParameter;
                SourceValuesErrors = null;
                try
                {
                    var multiConverter = converter as IMultiValueConverter;
                    if (multiConverter != null)
                    {
                        result = multiConverter.Convert(unconvertedValues, targetType, converterParameter, culture);
                    }
                    else
                    {
                        var singleConverter = converter as IValueConverter;
                        if (singleConverter != null)
                        {
                            result = singleConverter.Convert(unconvertedValues[0], targetType, converterParameter,
                                                             culture);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SourceValuesErrors = new[] {new MultiBindingValidationError(ex)};
                    if (MultiBinding.ValidatesOnExceptions)
                    {
                        return DependencyProperty.UnsetValue;
                    }
                    throw;
                }
            }


            if (result != DependencyProperty.UnsetValue)
            {
                String format = StringFormat;
                if (format != null)
                {
                    format = Regex.Replace(format, @"%(\d+)", "{$1}");
                    if (result is Object[])
                    {
                        result = String.Format(culture, format, (Object[]) result);
                    }
                    else
                    {
                        result = String.Format(culture, format, result);
                    }
                }
            }
            return result;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object converter = Converter;

            if (converter == null) return value;

            object converterParameter = ConverterParameter;
            SourceValuesErrors = null;
            try
            {
                if (converter is IMultiValueConverter)
                {
                    // Guess target types based on current source values
                    var targetTypes = new Type[m_sourceCount];
                    for (int i = 0; i < targetTypes.Length; i++)
                    {
                        object currentValue = Target.GetValue(dataProperties[m_sourceStartIndex + i]);
                        targetTypes[i] = currentValue != null ? currentValue.GetType() : typeof (Object);
                    }

                    object[] convertedValues = ((IMultiValueConverter) converter).ConvertBack(value, targetTypes,
                                                                                              converterParameter,
                                                                                              culture);

                    return convertedValues;
                }
                else if (converter is IValueConverter)
                {
                    object convertedValue = ((IValueConverter) converter).ConvertBack(value, targetType,
                                                                                      converterParameter, culture);
                    return convertedValue != DependencyProperty.UnsetValue ? new[] {convertedValue} : convertedValue;
                }
            }
            catch (Exception ex)
            {
                SourceValuesErrors = new[] {new MultiBindingValidationError(ex)};
                if (MultiBinding.ValidatesOnExceptions)
                {
                    return DependencyProperty.UnsetValue;
                }
                throw;
            }

            return value;
        }

        #endregion

        #region INotifyDataErrorInfo implementation

        private IEnumerable m_sourceValuesErrors;

        public IEnumerable SourceValuesErrors
        {
            get { return m_sourceValuesErrors; }
            set
            {
                if (m_sourceValuesErrors != value)
                {
                    m_sourceValuesErrors = value;
                    if (ErrorsChanged != null)
                    {
                        ErrorsChanged(this, new DataErrorsChangedEventArgs("SourceValues"));
                    }
                }
            }
        }


        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (propertyName == "SourceValues")
            {
                return SourceValuesErrors;
            }
            return null;
        }

        public bool HasErrors
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        private static List<MultiBindingExpression> GetMultiBindingExpressions(DependencyObject obj)
        {
            return (List<MultiBindingExpression>) obj.GetValue(MultiBindingExpressionsProperty);
        }

        private static void SetMultiBindingExpressions(DependencyObject obj, List<MultiBindingExpression> value)
        {
            obj.SetValue(MultiBindingExpressionsProperty, value);
        }

        private static DependencyProperty GetDataProperty(int dataPropertyIndex)
        {
            if (dataPropertyIndex < dataProperties.Count)
            {
                return dataProperties[dataPropertyIndex];
            }
            else
            {
                int i = dataProperties.Count;
                DependencyProperty dataProperty = null;
                while (i <= dataPropertyIndex)
                {
                    dataProperty = DependencyProperty.RegisterAttached("P" + i, typeof (Object),
                                                                       typeof (MultiBindingExpression),
                                                                       new PropertyMetadata(null, OnDataPropertyChanged));
                    dataProperties.Add(dataProperty);
                    i++;
                }
                return dataProperty;
            }
        }

        private void SetDataProperty(int dataPropertyIndex, object localValue)
        {
            DependencyProperty destProperty = GetDataProperty(dataPropertyIndex);

            if (localValue is BindingBase)
            {
                BindingOperations.SetBinding(Target, destProperty, (BindingBase) localValue);
            }
            else
            {
                var bindingExpression = localValue as BindingExpression;

                if (bindingExpression != null)
                {
                    Binding propertyBinding = bindingExpression.ParentBinding;
                    BindingOperations.SetBinding(Target, destProperty, propertyBinding);
                }
                else
                {
                    Target.SetValue(destProperty, localValue);
                }
            }
        }

        private static void OnDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            int propertyIndex = dataProperties.IndexOf(args.Property);


            List<MultiBindingExpression> mbExpressions = GetMultiBindingExpressions(d);
            if (mbExpressions != null)
            {
                foreach (MultiBindingExpression mbInfo in mbExpressions)
                {
                    if (mbInfo.HandleDataPropertyChanged(propertyIndex, args.NewValue))
                    {
                        break;
                    }
                }
            }
        }

        private void ProcessNewResult(object[] newValues)
        {
            if (newValues == null) return;

            for (int i = 0; i < m_sourceCount && i < newValues.Length; i++)
            {
                if (newValues[i] != DependencyProperty.UnsetValue)
                {
                    var bindingExpression =
                        Target.ReadLocalValue(dataProperties[m_sourceStartIndex + i]) as BindingExpression;
                    if (bindingExpression != null)
                    {
                        // Only update value if binding is two way., otherwise we would loose the binding to the source
                        if (bindingExpression.ParentBinding.Mode == BindingMode.TwoWay)
                        {
                            Target.SetValue(dataProperties[m_sourceStartIndex + i], newValues[i]);
                        }
                    }
                    else
                    {
                        Target.SetValue(dataProperties[m_sourceStartIndex + i], newValues[i]);
                    }
                }
            }
        }

        private void ApplyToTarget()
        {
            int dataPropertyIndex; // Next data property index to use

            // Get any existing MultiBindingInfo for this target
            List<MultiBindingExpression> mbExpressions = GetMultiBindingExpressions(Target);
            if (mbExpressions == null) // if no existing MultiBindings on this target
            {
                mbExpressions = new List<MultiBindingExpression>(1);

                dataPropertyIndex = 0;
            }
            else
            {
                // Fetch last used data property index from last expression.
                dataPropertyIndex = mbExpressions[mbExpressions.Count - 1].m_lastIndex + 1;
            }

            // Fill in this expression with information about this binding and all sources, converters and bindings.
            // and set attached data properties on the target as required.
            m_sourceStartIndex = dataPropertyIndex;

            foreach (DependencyProperty sourceProperty in MultiBinding.SourceProperties)
            {
                object localValue = MultiBinding.ReadLocalValue(sourceProperty);
                if (localValue == DependencyProperty.UnsetValue)
                {
                    break;
                }

                SetDataProperty(dataPropertyIndex++, localValue);
            }
            if (MultiBinding.Bindings != null && MultiBinding.Bindings.Count > 0)
            {
                if (dataPropertyIndex != m_sourceStartIndex)
                {
                    throw new InvalidOperationException(
                        "MutliBinding.Bindings cannot be used at the same time as Source-properties.");
                }
                for (int i = 0; i < MultiBinding.Bindings.Count; i++)
                {
                    SetDataProperty(dataPropertyIndex++, MultiBinding.Bindings[i]);
                }
            }
            m_sourceCount = dataPropertyIndex - m_sourceStartIndex;

            object converter = MultiBinding.ReadLocalValue(MultiBinding.ConverterProperty);
            if (converter is BindingExpression)
            {
                m_converterIndex = dataPropertyIndex++;
                SetDataProperty(m_converterIndex, converter);
            }

            object converterParameter = MultiBinding.ReadLocalValue(MultiBinding.ConverterParameterProperty);
            if (converterParameter is BindingExpression)
            {
                m_converterParameterIndex = dataPropertyIndex++;
                SetDataProperty(m_converterParameterIndex, converterParameter);
            }

            object stringFormat = MultiBinding.ReadLocalValue(MultiBinding.StringFormatProperty);
            if (stringFormat is BindingExpression)
            {
                m_stringFormatIndex = dataPropertyIndex++;
                SetDataProperty(m_stringFormatIndex, stringFormat);
            }

            m_lastIndex = dataPropertyIndex - 1;

            // Update MultiBindingInfoProperties for target 
            mbExpressions.Add(this);
            if (mbExpressions.Count == 1)
            {
                Target.SetValue(MultiBindingExpressionsProperty, mbExpressions);
            }
            // Queue up an update request to ensure that the SourceValues property will be set.
            BeginUpdate();
        }

        private void BeginUpdate()
        {
            if (m_pendingUpdateAction != null) return;
            m_pendingUpdateAction = Update;
            Target.Dispatcher.BeginInvoke(m_pendingUpdateAction);
        }

        private void Update()
        {
            var sources = new object[m_sourceCount];
            for (int i = 0; i < m_sourceCount; i++)
            {
                sources[i] = Target.GetValue(dataProperties[m_sourceStartIndex + i]);
            }

            SourceValues = sources;

            m_pendingUpdateAction = null;
        }

        private bool HandleDataPropertyChanged(int propertyIndex, object newValue)
        {
            if (propertyIndex >= m_sourceStartIndex && propertyIndex <= m_lastIndex)
            {
                BeginUpdate();
                return true;
            }
            return false;
        }
    }
}