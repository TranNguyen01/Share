using System.ComponentModel.DataAnnotations;

namespace Moto.Models.ValidationModels
{
    public class UserLoginValidation
    {
        public UserLoginValidation(string userName, string password, bool isRemember = false)
        {
            UserName = userName;
            Password = password;
            IsRemember = isRemember;
        }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool IsRemember { get; set; }

    }
}
