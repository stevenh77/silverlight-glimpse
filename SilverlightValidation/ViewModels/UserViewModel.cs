using System;
using System.Linq;
using FluentValidation;
using SilverlightValidation.Interfaces;
using SilverlightValidation.Validators;
using SilverlightValidation.Models;

namespace SilverlightValidation.ViewModels
{
    public class UserViewModel : ViewModelBase, IUserModel
    {
        #region Fields

        private readonly UserModelValidator _validator;
        private UserModel _data;

        #endregion

        #region Constructor

        public UserViewModel(UserModel model, UserModelValidator validator)
        {
            _validator = validator;
            _data = model;
        }

        #endregion

        #region Properties

        private const string UsernameProperty = "Username";
        public string Username
        {
            get { return _data.Username; }
            set
            {
                if (_data.Username != value)
                {
                    _data.Username = value;
                    RaisePropertyChanged(UsernameProperty);
                }

                ClearError(UsernameProperty);
                var validationResult = _validator.Validate(this, UsernameProperty);
                if (!validationResult.IsValid)
                    validationResult.Errors.ToList().ForEach(x => SetError(UsernameProperty, x.ErrorMessage));
            }
        }

        private const string PasswordProperty = "Password";
        public string Password
        {
            get { return _data.Password; }
            set
            {
                if (_data.Password != value)
                {
                    _data.Password = value;
                    RaisePropertyChanged(PasswordProperty);
                }

                ClearError(PasswordProperty);
                var validationResult = _validator.Validate(this, PasswordProperty);
                if (!validationResult.IsValid)
                    validationResult.Errors.ToList().ForEach(x => SetError(PasswordProperty, x.ErrorMessage));
            }
        }

        private const string EmailProperty = "Email";
        public string Email
        {
            get { return _data.Email; }
            set
            {
                if (_data.Email != value)
                {
                    _data.Email = value;
                    RaisePropertyChanged(EmailProperty);
                }

                ClearError(EmailProperty);
                var validationResult = _validator.Validate(this, EmailProperty);
                if (!validationResult.IsValid)
                    validationResult.Errors.ToList().ForEach(x => SetError(EmailProperty, x.ErrorMessage));
            }
        }

        private const string DateOfBirthProperty = "DateOfBirth";
        public DateTime? DateOfBirth
        {
            get { return _data.DateOfBirth; }
            set
            {
                if (_data.DateOfBirth != value)
                {
                    _data.DateOfBirth = value;
                    RaisePropertyChanged(DateOfBirthProperty);
                }

                ClearError(DateOfBirthProperty);
                var validationResult = _validator.Validate(this, DateOfBirthProperty);
                if (!validationResult.IsValid)
                    validationResult.Errors.ToList().ForEach(x => SetError(DateOfBirthProperty, x.ErrorMessage));
            }
        }

        private const string DescriptionProperty = "Description";
        public string Description
        {
            get { return _data.Description; }
            set
            {
                if (_data.Description != value)
                {
                    _data.Description = value;
                    RaisePropertyChanged(DescriptionProperty);
                }

                ClearError(DescriptionProperty);
                var validationResult = _validator.Validate(this, DescriptionProperty);
                if (!validationResult.IsValid)
                    validationResult.Errors.ToList().ForEach(x => SetError(DescriptionProperty, x.ErrorMessage));
            }
        }

        #endregion
    }
}
