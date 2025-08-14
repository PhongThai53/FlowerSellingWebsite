using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("Suppliers")]
    public class Supplier : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        // Navigation Properties
        public virtual ICollection<FlowerBatch> FlowerBatches { get; set; } = new List<FlowerBatch>();
        public virtual ICollection<SupplierListing> SupplierListings { get; set; } = new List<SupplierListing>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
