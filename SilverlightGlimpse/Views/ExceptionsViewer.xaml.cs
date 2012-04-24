using SilverlightGlimpse.Services;

namespace SilverlightGlimpse.Views 
{
    public partial class ExceptionsViewer
    {
        public ExceptionsViewer()
        {
            InitializeComponent();
            lbExceptions.ItemsSource = Glimpse.Service.Exceptions;
        }
    }
}