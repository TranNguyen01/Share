using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class Product
    {
        public Product()
        {
            ProductImages = new HashSet<ProductImage>();
            Carts = new HashSet<Cart>();
            OrderDetails = new HashSet<OrderDetail>();
            CreatedDate = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

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

        public DateTime CreatedDate { get; set; }

        [Required]
        [Range(minimum: 0, maximum: double.MaxValue)]
        public double Price { get; set; }

        [Required]
        [Range(minimum: 0, maximum: int.MaxValue)]
        public int Quantity { get; set; }

        public bool IsDeleted { get; set; }

        public virtual Brand? Brand { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
