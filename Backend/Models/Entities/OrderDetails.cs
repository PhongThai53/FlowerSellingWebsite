using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class OrderDetails : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [ForeignKey("FlowerBatch")]
        public int? FlowerBatchId { get; set; }

        [ForeignKey("SupplierListing")]
        public int? SupplierListingId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public string? ItemName { get; set; }

        [Required]
        public int Quantity { get; set; }
        public int? ApprovedQuantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }
        public decimal? FinalUnitPrice { get; set; }

        [Required]
        public decimal Discount { get; set; }

        [Required]
        public decimal LineTotal { get; set; }
        public decimal? FinalAmount { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        public virtual Orders Order { get; set; } = null!;
        public virtual Products Product { get; set; } = null!;
        public virtual FlowerBatches? FlowerBatch { get; set; }
        public virtual SupplierListings? SupplierListing { get; set; }
    }
}
