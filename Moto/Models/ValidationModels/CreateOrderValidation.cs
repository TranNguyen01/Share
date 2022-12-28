namespace Moto.Models.ValidationModels
{
    public class CreateOrderValidation
    {
        public string UserId { get; set; }

        public int AddressId { get; set; }

        public virtual ICollection<OrderDetailValidation> Details { get; set; }
    }
}
