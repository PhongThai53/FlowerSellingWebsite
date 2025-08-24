using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Cart : BaseEntity
    {
        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public bool IsCheckedOut { get; set; } = false;

        // Navigation properties
        public virtual Users User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
    }
}