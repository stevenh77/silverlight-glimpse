using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SilverlightGlimpse.Diagnostics;
using SilverlightGlimpse.Interfaces;
using SilverlightGlimpse.Views;
using SilverlightGlimpse.Models;
using System.Windows.Media;

namespace SilverlightGlimpse.Services
{
    public class Glimpse
    {
        #region Fields

        private static Glimpse _instance;
        private DispatcherTimer _refreshBindingCountTimer;
        private static readonly TimeSpan FiveSeconds = new TimeSpan(0, 0, 0, 5);
        private const int MAX_LOG_ITEMS = 9;
        private const string SILVERLIGHT_GLIMPSE = "Silverlight Glimpse";
        private readonly ILogWriter _logWriter;
        private readonly IBrokenBindingsService _brokenBindingsService;

        #endregion

        #region Private constructor

        private Glimpse(ILogWriter logWriter, IBrokenBindingsService brokenBindingsService)
        {
            BindingErrors = new ObservableCollection<BindingError>();
            ValidationErrors = new ObservableCollection<ValidationWrapper>();
            Exceptions = new ObservableCollection<Exception>();
            Log = new ObservableCollection<string>();
            _logWriter = logWriter;
            _brokenBindingsService = brokenBindingsService;
        }

        #endregion

        #region Properties

        public static Glimpse Service { get { return _instance ?? (_instance = new Glimpse(new LogWriter(), new BrokenBindingsService())); } }
        internal Application App { get; private set; }
        internal FloatableWindow GlimpseWindow { get; set;}
        internal string Title { get; set; }
        public ObservableCollection<BindingError> BindingErrors { get; private set; }
        public ObservableCollection<ValidationWrapper> ValidationErrors { get; private set; }
        public ObservableCollection<Exception> Exceptions { get; private set; }
        internal FrameworkElement RootVisual { get; private set; }
        internal TimeSpan BindingsRefreshRate { get; private set; }
        public ObservableCollection<string> Log { get; private set; }
        public bool IsInDebugMode
        {
            get { 
#if DEBUG
                return true;
#endif
#pragma warning disable 162
                return false;
#pragma warning restore 162
            }
        }

        #endregion

        #region Methods

        // Used when application fails to load a rootpage due to an exception.
        // Silverlight Glimpse becomes the new rootpage.
        public void DisplayLoadFailure(Application app, Exception ex)
        {
            var source = ex.StackTrace.Substring(0, ex.StackTrace.IndexOf(Environment.NewLine, StringComparison.Ordinal));
            
            App = app;
            App.RootVisual = new LoadExceptionViewer(ex, source);
            Title = SILVERLIGHT_GLIMPSE;
            Debug.WriteLine("{0} had exception. {1}", Title, ex);
        }


        // Normal loading of Silverlight Glimpse.
        public void Load(Application app, TimeSpan bindingsRefreshRate = default(TimeSpan))
        {
            App = app;
            RootVisual = App.RootVisual as FrameworkElement;
            if (RootVisual == null) throw new NullReferenceException("The Application provided did not have a valid RootVisual object");
            Title = SILVERLIGHT_GLIMPSE;
            BindingsRefreshRate = (bindingsRefreshRate == default(TimeSpan)) ? FiveSeconds : bindingsRefreshRate;
            
            RootVisual.BindingValidationError += HostRootVisual_BindingValidationError;
            App.UnhandledException += Application_UnhandledException;

            GlimpseWindow = new FloatableWindow
            {
                Title = Title,
                Content = new GlimpseViewer(),
                ParentLayoutRoot = (Panel) VisualTreeHelper.GetChild(RootVisual, 0)
            };

            if (Double.IsNaN(RootVisual.Width))
            {
                //if the host control is autosized (consumes the browser window) then locate Glimpse in the top, left
                GlimpseWindow.Show();
            }
            else
            {
                 //if the host is fixed size then attempt to locate the popup control in the upper right corner
                var dblLeft = RootVisual.Width - 200;

                if (dblLeft < 0)
                    dblLeft = 0;

                GlimpseWindow.Show(dblLeft, 10);
            }

            _brokenBindingsService.LoadBrokenBindings(RootVisual, BindingErrors);

            _refreshBindingCountTimer = new DispatcherTimer();
            _refreshBindingCountTimer.Tick += RefreshBindingCountTimer_Tick;
            _refreshBindingCountTimer.Interval = BindingsRefreshRate;
            _refreshBindingCountTimer.Start();
        }

        public void Clear()
        {
            BindingErrors.Clear();
            Log.Clear();
            ValidationErrors.Clear();
            Exceptions.Clear();
        }

        #endregion

        #region Events handlers

        // Bindings
        private void RefreshBindingCountTimer_Tick(object sender, EventArgs e)
        {
            var stopwatch = Stopwatch.StartNew();

            _refreshBindingCountTimer.Stop();

            BindingErrors.Clear();
            _brokenBindingsService.LoadBrokenBindings(RootVisual, BindingErrors);

            stopwatch.Stop();
            _logWriter.Write(Log, MAX_LOG_ITEMS, "Refreshing the binding errors took {0} {1}", stopwatch.ElapsedTime.Milliseconds, "ms");

            _refreshBindingCountTimer.Start();
        }

        // Validation
        private void HostRootVisual_BindingValidationError(object sender, ValidationErrorEventArgs e)
        {
            var controlName = "(none)";
            var control = e.OriginalSource as Control;

            if (control != null && !string.IsNullOrEmpty(control.Name))
            {
                controlName = control.Name;
            }

            ValidationErrors.Add(
                new ValidationWrapper(
                    e.Action, 
                    controlName,
                    e.OriginalSource.GetType().Name, 
                    e.Error.ErrorContent.ToString()));
        }

        // Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("{0} had exception.  {1}", Title, e.ExceptionObject);

            Exception ex = e.ExceptionObject;

            while (ex != null)
            {
                // TODO: Decide whether inner exceptions should be viewable separately
                this.Exceptions.Add(ex);
                ex = ex.InnerException;
            }

            e.Handled = true;
        }

        #endregion
    }
}