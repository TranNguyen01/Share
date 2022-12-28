using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }
        public User User { get; set; }
    }
}
