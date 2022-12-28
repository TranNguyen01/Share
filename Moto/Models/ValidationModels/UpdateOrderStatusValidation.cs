using System.ComponentModel.DataAnnotations;

namespace Moto.Models.ValidationModels
{
    public class UpdateOrderStatusValidation
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [Range(-3, 3)]
        public int Status { get; set; }

        [Required]
        [Range(-3, 3)]
        public int OldStatus { get; set; }
    }
}
