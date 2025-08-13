using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public required string UserName { get; set; }

        [MaxLength(256)]
        public string? PasswordHash { get; set; }

        [MaxLength(150)]
        public string? FullName { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public Guid RoleId { get; set; }

        // Navigation Properties
        public virtual Role? Role { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
