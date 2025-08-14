using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class RolePermissions : BaseEntity
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        // Navigation Properties
        [ForeignKey("RoleId")]
        public virtual Roles Role { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permissions Permission { get; set; } = null!;
    }
}
