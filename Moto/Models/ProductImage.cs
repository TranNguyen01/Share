using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public virtual Product Product { get; set; }

        [Required]
        public int ImageId { get; set; }

        public virtual Image Image { get; set; }
    }
}
