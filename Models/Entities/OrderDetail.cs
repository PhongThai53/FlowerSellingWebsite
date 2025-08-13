namespace FlowerSellingWebsite.Models.Entities
{
    public class OrderDetail : BaseEntity
    {
        public Guid OrderId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
    }
}
