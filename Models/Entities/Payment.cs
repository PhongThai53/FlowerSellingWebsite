using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }

        public Guid PaymentMethodId { get; set; }

        [MaxLength(50)]
        public string? PaymentStatus { get; set; }

        public decimal Amount { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }
    }
}
