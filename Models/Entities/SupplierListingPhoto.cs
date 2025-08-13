using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("SupplierListingPhotos")]
    public class SupplierListingPhoto : BaseEntity
    {
        [Required]
        public int SupplierListingId { get; set; }

        [Required]
        [StringLength(300)]
        public string PhotoUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; } = false;

        [StringLength(200)]
        public string? PhotoDescription { get; set; }

        // Navigation Properties
        [ForeignKey("SupplierListingId")]
        public virtual SupplierListing SupplierListing { get; set; } = null!;
    }
}
