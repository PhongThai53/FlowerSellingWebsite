using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerTypes : BaseEntity
    {
        [Required]
        public string TypeName { get; set; } = null!;
        public string? Description { get; set; }

        // Navigation properties - removed FlowerCategoryImages and FlowerPricings
        public virtual ICollection<Flowers> Flowers { get; set; } = new HashSet<Flowers>();
    }
}
