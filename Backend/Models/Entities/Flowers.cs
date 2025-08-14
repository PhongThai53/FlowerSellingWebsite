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
        public int? FlowerCategoryId { get; set; }  // Nullable based on diagram comment "optional"

        [Required]
        [ForeignKey("FlowerType")]
        public int FlowerTypeId { get; set; }

        [Required]
        [ForeignKey("FlowerColor")]
        public int FlowerColorId { get; set; }

        public string? Size { get; set; }
        [Required]
        public int ShelfLifeDays { get; set; } // days to keep from import date

        // Navigation properties
        public virtual FlowerCategories? FlowerCategory { get; set; }
        public virtual FlowerTypes FlowerType { get; set; } = null!;
        public virtual FlowerColors FlowerColor { get; set; } = null!;
        public virtual ICollection<FlowerBatches> FlowerBatches { get; set; } = new HashSet<FlowerBatches>();
        public virtual ICollection<ProductFlowers> ProductFlowers { get; set; } = new HashSet<ProductFlowers>();
        public virtual ICollection<PurchaseOrderDetails> PurchaseOrderDetails { get; set; } = new HashSet<PurchaseOrderDetails>();
    }

}
