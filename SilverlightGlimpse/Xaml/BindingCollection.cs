// Original code:  http://www.codeproject.com/Articles/286171/MultiBinding-in-Silverlight-5
// Author:  Henrik Jonsson

using System;
using System.Collections.ObjectModel;

namespace SilverlightGlimpse.Xaml
{
    public class BindingCollection : Collection<Object>
    {
        public bool IsSealed { get; private set; }

        public void Seal()
        {
            IsSealed = true;
        }

        public void CheckSealed()
        {
            if (IsSealed)
                throw new InvalidOperationException("This BindingCollection cannot be changed when it has been sealed.");
        }

        protected override void InsertItem(int index, Object item)
        {
            CheckSealed();
            base.InsertItem(index, item);
        }

        protected override void ClearItems()
        {
            CheckSealed();
            base.ClearItems();
        }

        protected override void SetItem(int index, object item)
        {
            CheckSealed();
            base.SetItem(index, item);
        }
    }
}
