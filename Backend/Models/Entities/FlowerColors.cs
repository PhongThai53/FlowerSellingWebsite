using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerColors : BaseEntity
    {
        [Required]
        public string? ColorName { get; set; }
        [Required]
        public string? HexCode { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<Flowers> Flowers { get; set; } = new HashSet<Flowers>();
    }
}
