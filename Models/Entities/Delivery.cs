using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    [Table("Deliveries")]
    public class Delivery : BaseEntity
    {
        [Required]
        public int OrderId { get; set; }

        [StringLength(50)]
        public string? DeliveryStatus { get; set; } = "Pending";

        [StringLength(300)]
        public string? DeliveryAddress { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(500)]
        public string? DeliveryNotes { get; set; }

        // Navigation Properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;
    }
}
