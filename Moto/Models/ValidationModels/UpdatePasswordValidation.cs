namespace Moto.Models.ValidationModels
{
    public class UpdatePasswordValidation
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }
}
