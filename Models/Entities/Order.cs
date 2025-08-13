namespace FlowerSellingWebsite.Models.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }

        public Guid OrderId { get; set; }

        public decimal TotalAmount { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
        public virtual ICollection<SystemLog> SystemLogs { get; set; } = new List<SystemLog>();
    }
}
