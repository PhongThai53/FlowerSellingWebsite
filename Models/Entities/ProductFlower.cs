namespace FlowerSellingWebsite.Models.Entities
{
    public class ProductFlower : BaseEntity
    {
        public Guid ProductId { get; set; }

        public Guid FlowerId { get; set; }

        public int QuantityUsed { get; set; }

        // Navigation Properties
        public virtual Product? Product { get; set; }
        public virtual Flower? Flower { get; set; }
    }
}
