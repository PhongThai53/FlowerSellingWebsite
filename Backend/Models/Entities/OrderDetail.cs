using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("OrderDetails")]
    public class OrderDetail : BaseEntity
    {
        [Required]
        public int OrderId { get; set; }

        public int? ProductId { get; set; }

        public int? FlowerBatchId { get; set; }

        public int? SupplierListingId { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RequestedQuantity must be positive")]
        public int RequestedQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ApprovedQuantity must be non-negative")]
        public int? ApprovedQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be positive")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "FinalUnitPrice must be positive")]
        public decimal? FinalUnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "EstimatedAmount must be positive")]
        public decimal EstimatedAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "FinalAmount must be positive")]
        public decimal? FinalAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [ForeignKey("FlowerBatchId")]
        public virtual FlowerBatch? FlowerBatch { get; set; }

        [ForeignKey("SupplierListingId")]
        public virtual SupplierListing? SupplierListing { get; set; }
    }

}
