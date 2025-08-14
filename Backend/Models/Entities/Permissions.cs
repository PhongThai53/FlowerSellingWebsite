using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Permissions : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
    }
}
