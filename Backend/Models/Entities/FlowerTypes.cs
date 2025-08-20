using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerTypes : BaseEntity
    {
        [Required]
        public string? TypeName { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Flowers> Flowers { get; set; } = new HashSet<Flowers>();
    }
}
