using System;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Input;
using SilverlightValidation.Models;
using SilverlightValidation.Validators;
using SilverlightValidation.ViewModels;

namespace SilverlightValidation.Views
{
    public partial class UserView
    {
        private UserViewModel vm;

        public UserView()
        {
            InitializeComponent();
            HtmlPage.Document.SetProperty("title", "Silverlight Validation");

            vm = new UserViewModel(UserModel.Create(), new UserModelValidator());
            this.DataContext = vm;

            // uncomment this exception to view exception on startup 
            //ThrowException();
        }

        private void DatePicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Tab)
                e.Handled = true;
        }

        private void btnThrowException_OnClick(object sender, RoutedEventArgs e)
        {
            ThrowException();
        }

        private void ThrowException()
        {
            throw new Exception("Oh dear we've hit an exception!");
        }
    }
}