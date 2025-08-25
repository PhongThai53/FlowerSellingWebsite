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
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; }

        [Required]
        [ForeignKey("Roles")]
        public int RoleId { get; set; }

        // Navigation properties
        public virtual Suppliers? Supplier { get; set; }
        public virtual Roles Role { get; set; } = null!;
        public virtual ICollection<Orders> Orders { get; set; } = new HashSet<Orders>();
        public virtual ICollection<FlowerDamageLogs> FlowerDamageLogs { get; set; } = new HashSet<FlowerDamageLogs>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
