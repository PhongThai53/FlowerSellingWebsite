using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Flowers : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }

        [ForeignKey("FlowerCategory")]
        public int? FlowerCategoryId { get; set; }

        [Required]
        [ForeignKey("FlowerType")]
        public int FlowerTypeId { get; set; }

        [Required]
        [ForeignKey("FlowerColor")]
        public int FlowerColorId { get; set; }

        public string? Size { get; set; }

        [Required]
        public int ShelfLifeDays { get; set; }

        // Navigation properties
        public virtual FlowerCategories? FlowerCategory { get; set; }
        public virtual FlowerTypes? FlowerType { get; set; }
        public virtual FlowerColors? FlowerColor { get; set; }
        public virtual ICollection<ProductFlowers> ProductFlowers { get; set; } = new HashSet<ProductFlowers>();

        // ❌ Bỏ đi PurchaseOrderDetails
        // public virtual ICollection<PurchaseOrderDetails> PurchaseOrderDetails { get; set; } = new HashSet<PurchaseOrderDetails>();

        public virtual ICollection<FlowerImages> FlowerImages { get; set; } = new HashSet<FlowerImages>();
        public virtual ICollection<FlowerPricing> FlowerPricings { get; set; } = new HashSet<FlowerPricing>();
        public virtual ICollection<SupplierListings> SupplierListings { get; set; } = new HashSet<SupplierListings>();  // 🔥 để xem tất cả supplier nào bán hoa này
    }
}
