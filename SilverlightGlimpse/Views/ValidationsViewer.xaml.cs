using SilverlightGlimpse.Services;

namespace SilverlightGlimpse.Views 
{
    public partial class ValidationsViewer
    {
        public ValidationsViewer()
        {
            InitializeComponent();
            lbValidationErrors.ItemsSource = Glimpse.Service.ValidationErrors;
        }
    }
}