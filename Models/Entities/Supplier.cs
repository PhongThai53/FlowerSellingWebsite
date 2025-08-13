using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Supplier : BaseEntity
    {
        [Required]
        [MaxLength(150)]
        public required string SupplierName { get; set; }

        [MaxLength(100)]
        public string? ContactPerson { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        // Navigation Properties
        public virtual ICollection<Flower> Flowers { get; set; } = new List<Flower>();
    }
}
