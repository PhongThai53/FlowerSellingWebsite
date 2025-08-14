using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class SupplierListingPhotos : BaseEntity
    {
        [Required]
        [ForeignKey("SupplierListing")]
        public int SupplierListingId { get; set; }

        [Required]
        public string Url { get; set; } = null!;
        [Required]
        public bool IsPrimary { get; set; }

        // Navigation properties
        public virtual SupplierListings SupplierListing { get; set; } = null!;
    }
}
