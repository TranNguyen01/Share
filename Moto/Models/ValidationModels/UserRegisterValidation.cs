using Moto.Models.ValidationModels;
using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class UserRegisterValidation : UserInfomationValidation
    {
        public UserRegisterValidation(string email, string phoneNumber, string lastName, string firstName, DateTime birthDate, string password) : base(phoneNumber, lastName, firstName, birthDate)
        {
            Password = password;
            Email = email;
        }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
