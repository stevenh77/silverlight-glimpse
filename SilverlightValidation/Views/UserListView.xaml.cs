using System;
using System.Windows.Browser;
using System.Windows.Input;
using SilverlightValidation.Data;
using SilverlightValidation.ViewModels;
using SilverlightValidation.Validators;

namespace SilverlightValidation.Views
{
    public partial class UserListView
    {
        private readonly UserListViewModel vm;

        public UserListView()
        {
            InitializeComponent();
            HtmlPage.Document.SetProperty("title", "Silverlight Glimpse Demo");

            vm = new UserListViewModel(Factory.CreateUserModels(), new UserModelValidator());
            this.DataContext = vm;
        }

        private void DatePicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab)
                e.Handled = true;
        }

        private void btnThrowException_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            throw new Exception("Oh dear we've hit an exception!",
                                new Exception("This is an inner exception"));
        }
    }
}
