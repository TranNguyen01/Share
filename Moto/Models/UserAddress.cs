namespace Moto.Models
{
    public class UserAddress : Address
    {
        public string UserId { get; set; }
        public virtual User? User { get; set; }
    }
}
