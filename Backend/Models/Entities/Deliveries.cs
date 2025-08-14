using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Deliveries : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; }
        [Required]
        public string DeliveryStatus { get; set; } = null!;
        public string? TrackingNumber { get; set; }
        public string? ShipperName { get; set; }

        // Navigation properties
        public virtual Orders Order { get; set; } = null!;
    }
}
