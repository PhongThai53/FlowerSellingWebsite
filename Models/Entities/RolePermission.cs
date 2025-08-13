namespace FlowerSellingWebsite.Models.Entities
{
    public class RolePermission : BaseEntity
    {
        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }

        // Navigation Properties
        public virtual Role? Role { get; set; }
        public virtual Permission? Permission { get; set; }
    }
}
