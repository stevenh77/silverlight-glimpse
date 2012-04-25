// Original code:  http://www.codeproject.com/Articles/286171/MultiBinding-in-Silverlight-5
// Author:  Henrik Jonsson

using System;

namespace SilverlightGlimpse.Xaml
{
    public class MultiBindingValidationError
    {
        public MultiBindingValidationError(Exception exception)
        {
            if (exception == null) throw new ArgumentNullException();
            Exception = exception;
        }

        public Exception Exception { get; private set; }

        public override string ToString() { return Exception.Message; }
    }
}