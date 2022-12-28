using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class UserValidation
    {
        public UserValidation(string email, string phoneNumber, string lastName, string firstName, DateTime birthDate, string password)
        {
            Email = email;
            PhoneNumber = phoneNumber;
            LastName = lastName;
            FirstName = firstName;
            BirthDate = birthDate;
            Password = password;
        }

        [EmailAddress]
        public string Email { get; set; }

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
        public DateTime BirthDate { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
