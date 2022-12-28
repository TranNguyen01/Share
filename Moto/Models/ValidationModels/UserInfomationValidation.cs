using System.ComponentModel.DataAnnotations;

namespace Moto.Models.ValidationModels
{
    public class UserInfomationValidation
    {
        public UserInfomationValidation(string phoneNumber, string lastName, string firstName, DateTime birthDate)
        {
            PhoneNumber = phoneNumber;
            LastName = lastName;
            FirstName = firstName;
            Birthdate = birthDate;
        }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public bool Gender { get; set; }

        [Required]
        public DateTime Birthdate { get; set; }

    }
}
