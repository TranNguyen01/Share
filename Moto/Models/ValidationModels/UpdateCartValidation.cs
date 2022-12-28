namespace Moto.Models.ValidationModels
{
    public class UpdateCartValidation
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int OldQuantity { get; set; }
        public int Quantity { get; set; }
    }
}
