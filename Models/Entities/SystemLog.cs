using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("SystemLogs")]
    public class SystemLog : BaseEntity
    {
        [Required]
        public int UserId { get; set; }

        [StringLength(100)]
        public string? Action { get; set; }

        public int RecordId { get; set; }

        [StringLength(100)]
        public string? TableName { get; set; }

        [StringLength(45)]
        public string? IPAddress { get; set; }

        public int? OrderId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
