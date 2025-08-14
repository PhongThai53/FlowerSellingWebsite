using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerDamageLogs : BaseEntity
    {
        [Required]
        [ForeignKey("FlowerBatch")]
        public int FlowerBatchId { get; set; }

        [Required]
        [ForeignKey("ReportedByUser")]
        public int ReportedByUserId { get; set; }

        [Required]
        public int DamagedQuantity { get; set; }
        [Required]
        public string DamageReason { get; set; } = null!;
        [Required]
        public DateTime DamageDate { get; set; }
        public string? Notes { get; set; }

        // Navigation properties
        public virtual FlowerBatches FlowerBatch { get; set; } = null!;
    }
}
