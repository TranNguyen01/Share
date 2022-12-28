using System.ComponentModel.DataAnnotations;

namespace Moto.Models.ValidationModels
{
    public class ProductValidation
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        [Required]
        [Range(minimum: 0, maximum: int.MaxValue)]
        public int ModelYear { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Range(minimum: 0, maximum: double.MaxValue)]
        public double Price { get; set; }

        [Required]
        [Range(minimum: 0, maximum: int.MaxValue)]
        public int Quantity { get; set; }

        ICollection<IFormFile> Images { get; set; }
    }
}
