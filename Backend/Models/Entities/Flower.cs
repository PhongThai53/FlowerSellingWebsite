using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("Flowers")]
    public class Flower : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Description { get; set; }


        [Required]
        public int FlowerCategoryId { get; set; }


        [StringLength(50)]
        public string? Color { get; set; }

        [StringLength(50)]
        public string? Size { get; set; }

        [ForeignKey("FlowerCategoryId")]
        public virtual FlowerCategory FlowerCategory { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<FlowerBatch> FlowerBatches { get; set; } = new List<FlowerBatch>();
        public virtual ICollection<FlowerPhoto> FlowerPhotos { get; set; } = new List<FlowerPhoto>();
        public virtual ICollection<ProductFlower> ProductFlowers { get; set; } = new List<ProductFlower>();
        public virtual ICollection<SupplierListing> SupplierListings { get; set; } = new List<SupplierListing>();
    }
}
