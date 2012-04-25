// Original code:  http://www.codeproject.com/Articles/286171/MultiBinding-in-Silverlight-5
// Author:  Henrik Jonsson

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace SilverlightGlimpse.Xaml
{
    [ContentProperty("Bindings")]
    public class MultiBinding : DependencyObject, IMarkupExtension<Object>, ISupportInitialize
    {
        private const int MaxStyleMultiBindingsCount = 8;

        private static readonly DependencyProperty[] styleMultBindingProperties;

        private static int currentStyleMultiBindingIndex;

        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter", typeof (Object), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

        public static readonly DependencyProperty ConverterParameterProperty =
            DependencyProperty.Register("ConverterParameter", typeof (object), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", typeof (String), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

        public static readonly DependencyProperty Source1Property =
            DependencyProperty.Register("Source1", typeof (object), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

        public static readonly DependencyProperty Source2Property =
            DependencyProperty.Register("Source2", typeof (object), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

       public static readonly DependencyProperty Source3Property =
            DependencyProperty.Register("Source3", typeof (object), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

        public static readonly DependencyProperty Source4Property =
            DependencyProperty.Register("Source4", typeof (object), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

        public static readonly DependencyProperty Source5Property =
            DependencyProperty.Register("Source5", typeof (object), typeof (MultiBinding),
                                        new PropertyMetadata(null, OnDependencyPropertyChanged));

        public static readonly DependencyProperty[] SourceProperties =
            new[] {Source1Property, Source2Property, Source3Property, Source4Property, Source5Property};

        private static readonly PropertyInfo SetterValueProperty = typeof (Setter).GetProperty("Value");
        private BindingMode m_bindingMode = BindingMode.OneWay;

        private BindingCollection m_bindings;
        private CultureInfo m_converterCulture;
        private object m_fallbackValue;
        private bool m_notifyOnValidationError;

        private DependencyProperty m_styleProperty;

        private object m_targetNullValue;
        private UpdateSourceTrigger m_updateSourceTrigger = UpdateSourceTrigger.Default;
        private bool m_validatesOnExceptions;

        static MultiBinding()
        {
            // Create Style Multi Binding attached properties
            styleMultBindingProperties = new DependencyProperty[MaxStyleMultiBindingsCount];
            for (int i = 0; i < MaxStyleMultiBindingsCount; i++)
            {
                styleMultBindingProperties[i] = DependencyProperty.RegisterAttached("StyleMultiBinding" + i,
                                                                                    typeof (MultiBinding),
                                                                                    typeof (MultiBinding),
                                                                                    new PropertyMetadata(null,
                                                                                                         OnStyleMultiBindingChanged));
            }
        }

        protected bool IsSealed { get; private set; }

        public BindingCollection Bindings
        {
            get { return m_bindings; }
            set { CheckSealed(); m_bindings = value; }
        }

        public BindingMode Mode
        {
            get { return m_bindingMode; }
            set { CheckSealed(); m_bindingMode = value; }
        }

        public object TargetNullValue
        {
            get { return m_targetNullValue; }
            set { CheckSealed(); m_targetNullValue = value; }
        }

        public CultureInfo ConverterCulture
        {
            get { return m_converterCulture; }
            set { CheckSealed(); m_converterCulture = value; }
        }

        public bool ValidatesOnExceptions
        {
            get { return m_validatesOnExceptions; }
            set { CheckSealed(); m_validatesOnExceptions = value; }
        }

        public bool NotifyOnValidationError
        {
            get { return m_notifyOnValidationError; }
            set { CheckSealed(); m_notifyOnValidationError = value; }
        }

        public object FallbackValue
        {
            get { return m_fallbackValue; }
            set { CheckSealed(); m_fallbackValue = value; }
        }

        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return m_updateSourceTrigger; }
            set { CheckSealed(); m_updateSourceTrigger = value; }
        }

        public Object Converter
        {
            get { return GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }

        public object ConverterParameter
        {
            get { return GetValue(ConverterParameterProperty); }
            set { SetValue(ConverterParameterProperty, value); }
        }

        public String StringFormat
        {
            get { return (String) GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        public object Source1
        {
            get { return GetValue(Source1Property); }
            set { SetValue(Source1Property, value); }
        }

        public object Source2
        {
            get { return GetValue(Source2Property); }
            set { SetValue(Source2Property, value); }
        }

        public object Source3
        {
            get { return GetValue(Source3Property); }
            set { SetValue(Source3Property, value); }
        }

        public object Source4
        {
            get { return GetValue(Source4Property); }
            set { SetValue(Source4Property, value); }
        }

        public object Source5
        {
            get { return GetValue(Source5Property); }
            set { SetValue(Source5Property, value); }
        }

        #region IMarkupExtension<object> Members

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var pvt = serviceProvider.GetService(typeof (IProvideValueTarget)) as IProvideValueTarget;
            if (pvt == null)
            {
                throw new InvalidOperationException(
                    "MultiBinding cannot determine the binding target object since the IProviderValueTarget service is unavailable.");
            }
            if (pvt.TargetObject is Setter && pvt.TargetProperty == SetterValueProperty)
            {
                var setter = (Setter) pvt.TargetObject;
                ApplyToStyle(setter);
                return this;
            }
            var target = pvt.TargetObject as DependencyObject;
            if (target == null)
            {
                throw new InvalidOperationException("MultiBinding can only be applied to DependencyObjects.");
            }
            Binding resultBinding = ApplyToTarget(target);

            return resultBinding.ProvideValue(serviceProvider);
        }

        #endregion

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
            // Nothing to do
        }

        void ISupportInitialize.EndInit()
        {
            bool isBindingsSet = Bindings != null && Bindings.Count > 0;
            bool unsetSourcePropertyFound = false;
            foreach (DependencyProperty sourceProperty in SourceProperties)
            {
                object sourceValue = ReadLocalValue(sourceProperty);
                if (sourceValue != DependencyProperty.UnsetValue)
                {
                    if (isBindingsSet)
                        throw new InvalidOperationException(
                            "Source properties and Bindings cannot be used at the same time.");
                    if (unsetSourcePropertyFound)
                        throw new InvalidOperationException("Source1, Source2, etc. properties must be used in order.");
                }
                else
                {
                    unsetSourcePropertyFound = true;
                }
            }
        }

        #endregion

        public void Seal()
        {
            IsSealed = true;
            if (Bindings != null) Bindings.Seal();
        }

        public void CheckSealed()
        {
            if (IsSealed)
                throw new InvalidOperationException(
                    "Properties on MultiBinding cannot be changed after it has been applied.");
        }

        private static void OnStyleMultiBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var mb = (MultiBinding) args.NewValue;
            if (mb != null)
            {
                // Only apply multibinding from Style if no local value has been set.
                object existingValue = d.ReadLocalValue(mb.m_styleProperty);
                if (existingValue == DependencyProperty.UnsetValue)
                {
                    // Apply binding to target by creating MultiBindingExpression and attached data properties.
                    Binding resultBinding = mb.ApplyToTarget(d);
                    BindingOperations.SetBinding(d, mb.m_styleProperty, resultBinding);
                }
            }
        }

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var mb = (MultiBinding) d;
            mb.CheckSealed();
        }

        private void ApplyToStyle(Setter setter)
        {
            m_styleProperty = setter.Property;
            if (m_styleProperty == null)
            {
                throw new InvalidOperationException(
                    "When a MultiBinding is applied to a Style setter the Property must be set first.");
            }
            setter.Property = styleMultBindingProperties[currentStyleMultiBindingIndex];
            currentStyleMultiBindingIndex = (currentStyleMultiBindingIndex + 1)%MaxStyleMultiBindingsCount;
            Seal();
        }

        public Binding ApplyToTarget(DependencyObject target)
        {
            Seal();

            // Create new MultiBindingInfo to hold information about this multibinding
            var newInfo = new MultiBindingExpression(target, this);

            // Create new binding to expressions's SourceValues property
            var path = new PropertyPath("SourceValues");
            var resultBinding = new Binding();
            resultBinding.Converter = newInfo;
            resultBinding.Path = path;
            resultBinding.Source = newInfo;
            resultBinding.Mode = Mode;
            resultBinding.UpdateSourceTrigger = UpdateSourceTrigger;
            resultBinding.TargetNullValue = TargetNullValue;
            resultBinding.ValidatesOnExceptions = ValidatesOnExceptions;
            resultBinding.ValidatesOnNotifyDataErrors = true;
            resultBinding.NotifyOnValidationError = NotifyOnValidationError;
            resultBinding.ConverterCulture = ConverterCulture;
            resultBinding.FallbackValue = FallbackValue;
            return resultBinding;
        }

        /*
         Tried to implement IList to allow direct XAML content but this failed in VS Designer when using Silverlight 5 RC. 

        int IList.Add(object value)
        {
            CheckSealed(); 
            Bindings.Add(value);
            return Bindings.Count - 1;
        }

        void IList.Clear()
        {
            CheckSealed();
            Bindings.Clear();
        }

        bool IList.Contains(object value)
        {
            return Bindings.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return Bindings.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            CheckSealed();
            Bindings.Insert(index, value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return IsSealed; }
        }

        void IList.Remove(object value)
        {
            CheckSealed();
            Bindings.Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            CheckSealed();
            Bindings.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return Bindings[index];
            }
            set
            {
                CheckSealed();
                Bindings[index] = value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((IList)Bindings).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return Bindings.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return Bindings; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Bindings.GetEnumerator();
        }  
         
         */
    }
}