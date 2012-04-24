using System.Collections.Generic;
using System.Windows;
using SilverlightGlimpse.Models;

namespace SilverlightGlimpse.Interfaces
{
    public interface IBrokenBindingsService
    {
        void LoadBrokenBindings(UIElement uiElement, IList<BindingError> bindingErrors);
    }
}