using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime UpdateAt { get; set; }

        public int AddressId { get; set; }

        public ShippingAddress Address { get; set; }

        public decimal Total { get; set; }

        public int status { get; set; }

        public virtual ICollection<OrderDetail> Details { get; set; }
        public virtual User Customer { get; set; }
    }
}
