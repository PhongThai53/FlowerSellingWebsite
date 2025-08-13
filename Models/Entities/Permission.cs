using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Permission : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public required string PermissionName { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
