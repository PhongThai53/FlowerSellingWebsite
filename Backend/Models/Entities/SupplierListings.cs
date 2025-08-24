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
        public int FlowerId { get; set; }   // 🔥 thêm liên kết đến hoa

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

        // Navigation properties
        public virtual Suppliers? Supplier { get; set; }
        public virtual Flowers? Flower { get; set; }   // 🔥 navigation sang hoa
    }
}
