namespace Moto.Models.ValidationModels
{
    public class AddToCartValidation
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
