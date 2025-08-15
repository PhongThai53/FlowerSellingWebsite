using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerColors : BaseEntity
    {
        [Required]
        public string ColorName { get; set; } = null!;
        [Required]
        public string HexCode { get; set; } = null!;
        public string? Description { get; set; }

        // Navigation properties - removed FlowerCategoryImages and FlowerPricings
        public virtual ICollection<Flowers> Flowers { get; set; } = new HashSet<Flowers>();
    }
}
