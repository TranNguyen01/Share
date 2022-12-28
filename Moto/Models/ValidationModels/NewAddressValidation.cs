namespace Moto.Models.ValidationModels
{
    public class NewAddressValidation
    {
        public int id { get; set; }
        public string UserId { get; set; }
        public int StateId { get; set; }
        public int DistrictId { get; set; }
        public int WardId { get; set; }
        public string NameContact { get; set; }
        public string PhoneContact { get; set; }
    }
}
