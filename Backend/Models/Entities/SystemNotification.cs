using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("SystemNotifications")]
    public class SystemNotification : BaseEntity
    {
        public int? RecipientUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Content { get; set; }

        [Required]
        [StringLength(50)]
        public string NotificationType { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ActionUrl { get; set; }

        [StringLength(100)]
        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime? ExpiresAt { get; set; }

        // Navigation Properties
        [ForeignKey("RecipientUserId")]
        public virtual User? RecipientUser { get; set; }
    }
}
