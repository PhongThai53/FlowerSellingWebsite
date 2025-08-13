using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Delivery : BaseEntity
    {
        public Guid OrderId { get; set; }

        [MaxLength(50)]
        public string? DeliveryStatus { get; set; }

        [MaxLength(300)]
        public string? DeliveryAddress { get; set; }

        public DateTime? DeliveryDate { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }
    }
}
