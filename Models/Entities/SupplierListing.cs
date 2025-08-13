using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("SupplierListings")]
    public class SupplierListing : BaseEntity
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int FlowerId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "AvailableQuantity must be non-negative")]
        public int AvailableQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "SupplierPrice must be positive")]
        public decimal SupplierPrice { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLifeDays must be positive")]
        public int ShelfLifeDays { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        [Required]
        public DateTime ListingDate { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiryDate { get; set; }

        // Navigation Properties
        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; } = null!;

        [ForeignKey("FlowerId")]
        public virtual Flower Flower { get; set; } = null!;

        public virtual ICollection<SupplierListingPhoto> SupplierListingPhotos { get; set; } = new List<SupplierListingPhoto>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
