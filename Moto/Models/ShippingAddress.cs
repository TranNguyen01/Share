namespace Moto.Models
{
    public class ShippingAddress : Address
    {
        public Order Order { get; set; }
    }
}
