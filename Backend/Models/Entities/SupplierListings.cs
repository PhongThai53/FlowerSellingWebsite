using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class SupplierListings : BaseEntity
    {
        [Required]
        [ForeignKey("Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        public int AvailableQuantity { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        [Required]
        public int ShelfLifeDays { get; set; }
        [Required]
        public int MinOrderQty { get; set; }
        [Required]
        public string Status { get; set; } = null!;

        // Navigation properties
        public virtual Suppliers Supplier { get; set; } = null!;
        public virtual ICollection<SupplierListingPhotos> SupplierListingPhotos { get; set; } = new HashSet<SupplierListingPhotos>();
    }
}
