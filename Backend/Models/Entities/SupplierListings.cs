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
        [ForeignKey("Flower")]
        public int FlowerId { get; set; }

        [Required]
        public int AvailableQuantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int ShelfLifeDays { get; set; }

        [Required]
        public int MinOrderQty { get; set; }

        [Required]
        public string Status { get; set; } = "pending";

        public virtual Suppliers? Supplier { get; set; }
        public virtual Flowers? Flower { get; set; }
    }
}
