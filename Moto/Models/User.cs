using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class User : IdentityUser
    {

        public User()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            Birthdate = new DateTime();
        }

        public User(string firstName, string lastName, DateTime birthdate, int avatarId, Image avatar, string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            Birthdate = birthdate;
            AvatarId = avatarId;
            Avatar = avatar;
            PhoneNumber = phoneNumber;
        }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateTime Birthdate { get; set; }

        public int? AvatarId { get; set; }

        public bool Gender { get; set; }

        public virtual Image? Avatar { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<UserAddress> Adresses { get; set; }
    }
}
