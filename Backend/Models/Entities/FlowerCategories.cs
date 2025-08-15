using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerCategories : BaseEntity
    {
        [Required]
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }

        // Navigation properties - removed FlowerCategoryImages and FlowerPricings
        public virtual ICollection<Flowers> Flowers { get; set; } = new HashSet<Flowers>();
        public virtual ICollection<FlowerPricing> FlowerPricings { get; set; } = new HashSet<FlowerPricing>();
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
    }
}
