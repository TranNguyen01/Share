using System.ComponentModel.DataAnnotations;

namespace Moto.Models
{
    public class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
            Name = "";
            Description = "";
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
