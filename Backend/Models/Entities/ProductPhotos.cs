using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.Entities
{
    public class ProductPhotos : BaseEntity
    {
        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Required]
        public string Url { get; set; } = null!;
        [Required]
        public bool IsPrimary { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Products? Product { get; set; }
    }
}
