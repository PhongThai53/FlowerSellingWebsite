using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("FlowerDamageLogs")]
    public class FlowerDamageLog : BaseEntity
    {
        [Required]
        public int FlowerBatchId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "DamageQuantity must be positive")]
        public int DamageQuantity { get; set; }

        [StringLength(300)]
        public string? DamageReason { get; set; }

        [Required]
        public int ReportedByUserId { get; set; }

        [Required]
        public DateTime DamageDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("FlowerBatchId")]
        public virtual FlowerBatch FlowerBatch { get; set; } = null!;

        [ForeignKey("ReportedByUserId")]
        public virtual User ReportedByUser { get; set; } = null!;
    }
}
