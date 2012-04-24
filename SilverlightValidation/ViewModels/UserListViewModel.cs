using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using SilverlightValidation.Commands;
using SilverlightValidation.Models;
using SilverlightValidation.Validators;
using SilverlightValidation.Views;
using GalaSoft.MvvmLight.Messaging;
using SilverlightValidation.Messages;

namespace SilverlightValidation.ViewModels
{
    public class UserListViewModel
    {
        UserView window;

        public UserListViewModel(IList<UserModel> models, UserModelValidator validator)
        {
            Data = new ObservableCollection<UserViewModel>();

            foreach (var model in models)
                Data.Add(new UserViewModel(model, validator));

            AddCommand = new RelayCommand(AddCommandExecute);
            DeleteCommand = new RelayCommand(DeleteCommandExecute);

            Messenger.Default.Register<UserViewResponseMessage>(this, UserViewResponseMessageReceived);
        }

        private void UserViewResponseMessageReceived(UserViewResponseMessage userViewResponseMessage)
        {
            if (userViewResponseMessage.UserViewModel != null)
                Data.Add(userViewResponseMessage.UserViewModel);
            window.Close();
        }

        #region Properties

        public ObservableCollection<UserViewModel> Data { get; set; }

        public UserViewModel SelectedItem { get; set; }

        #endregion

        #region Commands

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; set; }

        private void AddCommandExecute(object obj)
        {
            window = new UserView();
            window.Show();
        }

        private void DeleteCommandExecute(object obj)
        {
            if (SelectedItem!=null)
                Data.Remove(SelectedItem);
        }

        #endregion
    }
}