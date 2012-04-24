using System;
using SilverlightValidation.Interfaces;

namespace SilverlightValidation.Models
{
    public class UserModel : IUserModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Description { get; set; }

        public static UserModel Create()
        {
            return new UserModel() { Username = "", Email = "", Password = "", DateOfBirth = null, Description = "" };
        }

        public UserModel Clone()
        {
            return (UserModel) this.MemberwiseClone(); 
        }
    }
}
