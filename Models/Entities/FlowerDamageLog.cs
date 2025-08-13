using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerDamageLog : BaseEntity
    {
        public Guid FlowerBatchDetailId { get; set; }

        public int DamageQuantity { get; set; }

        [MaxLength(300)]
        public string? DamageReason { get; set; }

        public Guid ReportedBy { get; set; }

        // Navigation Properties
        public virtual FlowerBatchDetail? FlowerBatchDetail { get; set; }
    }
}
