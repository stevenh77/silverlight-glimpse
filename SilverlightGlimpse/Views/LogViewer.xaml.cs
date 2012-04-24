using SilverlightGlimpse.Services;

namespace SilverlightGlimpse.Views
{
    public partial class LogViewer
    {
        public LogViewer()
        {
            InitializeComponent();
            lbLog.ItemsSource = Glimpse.Service.Log;
        }
    }
}