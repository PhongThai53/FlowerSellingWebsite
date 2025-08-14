using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Users : BaseEntity
    {
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        public string PasswordHash { get; set; } = null!;
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Address { get; set; }

        [Required]
        [ForeignKey("Roles")]
        public int RoleId { get; set; }

        // Navigation properties
        public virtual Roles Role { get; set; } = null!;
        public virtual ICollection<Orders> Orders { get; set; } = new HashSet<Orders>();
        public virtual ICollection<FlowerPriceHistory> FlowerPriceHistories { get; set; } = new HashSet<FlowerPriceHistory>();
    }
}
