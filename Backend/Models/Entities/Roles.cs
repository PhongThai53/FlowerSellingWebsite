using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Roles : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<Users> Users { get; set; } = new List<Users>();
        public virtual ICollection<RolePermissions> RolePermissions { get; set; } = new List<RolePermissions>();
    }
}
