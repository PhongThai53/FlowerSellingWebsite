using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class OrderDetails : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public string? ItemName { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public decimal LineTotal { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string? Notes { get; set; }

        // Navigation properties
        public virtual Orders? Order { get; set; }
        public virtual Products? Product { get; set; }
    }
}
