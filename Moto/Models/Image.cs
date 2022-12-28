using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class Image
    {
        public Image()
        {
            ProductImages = new HashSet<ProductImage>();
        }

        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PublicId { get; set; }
        public string Version { get; set; }
        public string Signature { get; set; }
        public string Format { get; set; }
        public string ResourceType { get; set; }
        public long Bytes { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public string SecureUrl { get; set; }
        public string Path { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
