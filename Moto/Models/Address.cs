using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class Address
    {
        [Key]
        public int id { get; set; }
        public int StateId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }

        public string Detail { get; set; }
        public string NameContact { get; set; }
        [Required]
        [Phone]
        public string PhoneContact { get; set; }
    }
}
