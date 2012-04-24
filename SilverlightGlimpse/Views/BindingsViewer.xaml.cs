using SilverlightGlimpse.Services;

namespace SilverlightGlimpse.Views
{
    public partial class BindingsViewer
    {
        public BindingsViewer()
        {
            InitializeComponent();
            lbBindings.ItemsSource = Glimpse.Service.BindingErrors;
        }
    }
}