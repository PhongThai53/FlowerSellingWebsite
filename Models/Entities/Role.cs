using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Role : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public required string RoleName { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
