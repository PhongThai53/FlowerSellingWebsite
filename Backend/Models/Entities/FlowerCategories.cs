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
    }
}
