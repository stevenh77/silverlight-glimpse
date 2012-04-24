using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using SilverlightGlimpse.Diagnostics;
using SilverlightGlimpse.Interfaces;
using SilverlightGlimpse.Models;

namespace SilverlightGlimpse.Services
{
    public class BrokenBindingsService : IBrokenBindingsService
    {
        public void LoadBrokenBindings(UIElement uiElement, IList<BindingError> bindingErrors)
        {
            var fe = uiElement as FrameworkElement;
            if (fe == null) return;

            foreach (var fi in fe.GetType().GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Static))
            {
                if (!ReferenceEquals(fi.FieldType, typeof(DependencyProperty))) continue;

                var be = fe.GetBindingExpression((DependencyProperty)fi.GetValue(null));

                if (be == null || be.ParentBinding.Source != null ||
                    be.ParentBinding.RelativeSource != null) continue;

                var isInherited = false;

                if (fe.DataContext != null && !string.IsNullOrEmpty(be.ParentBinding.Path.Path))
                {
                    foreach (var propertyInfo in fe.DataContext.GetType().GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Instance))
                    {
                        if (String.Compare(propertyInfo.Name, be.ParentBinding.Path.Path) != 0) continue;
                        isInherited = true;
                        break;
                    }
                }

                if (isInherited) break;

                //this code handles empty bindings on the Button controls
                // unsure as to why the Button has an empty or unresolved binding of textblock type...
                if ((fe.Name == "")
                    && (fe.GetType().Name == "TextBlock")
                    && (fi.Name == "TextProperty")
                    && (be.ParentBinding.Path.Path == ""))
                {
                    continue;
                }

                var value = PropertyPathHelper.GetValue(fe, "Text");
                if ((value!=null) && value.ToString() != string.Empty)
                    continue;

                var brokenBinding = new BindingError(
                    fe.Name,
                    fe.GetType().Name,
                    fi.Name,
                    be.ParentBinding.Path.Path);

                bindingErrors.Add(brokenBinding);
                Debug.WriteLine("Broken Binding: {0}", brokenBinding);
            }

            int children = VisualTreeHelper.GetChildrenCount(fe);

            for (int j = 0; j <= children - 1; j++)
            {
                var child = VisualTreeHelper.GetChild(fe, j) as FrameworkElement;

                if (child != null)
                {
                    LoadBrokenBindings(child, bindingErrors);
                }
            }
        }
    }
}
