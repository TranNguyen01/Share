using System.ComponentModel.DataAnnotations;

namespace Moto.Models.ValidationModels
{
    public class UpdateProductValidation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        [Range(minimum: 0, maximum: int.MaxValue)]
        public int ModelYear { get; set; }
        [Range(minimum: 0, maximum: double.MaxValue)]
        public double Price { get; set; }
        [Range(minimum: 0, maximum: int.MaxValue)]
        public int Quantity { get; set; }
    }
}
