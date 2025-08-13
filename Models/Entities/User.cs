using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(256)]
        public string? PasswordHash { get; set; }

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(50)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool IsCustomer { get; set; } = false;

        public bool IsSupplier { get; set; } = false;

        // Navigation Properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        public virtual ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
        public virtual ICollection<Order> CustomerOrders { get; set; } = new List<Order>();
        public virtual ICollection<FlowerDamageLog> DamageReports { get; set; } = new List<FlowerDamageLog>();
        public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
        public virtual ICollection<SystemNotification> Notifications { get; set; } = new List<SystemNotification>();
    }

}
