namespace FlowerSellingWebsite.Models.Entities
{
    public class FlowerBatchDetail : BaseEntity
    {
        public Guid FlowerBatchId { get; set; }

        public Guid FlowerId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int QuantityAvailable { get; set; }

        // Navigation Properties
        public virtual FlowerBatch? FlowerBatch { get; set; }
        public virtual Flower? Flower { get; set; }
        public virtual ICollection<FlowerDamageLog> FlowerDamageLogs { get; set; } = new List<FlowerDamageLog>();
    }
}
