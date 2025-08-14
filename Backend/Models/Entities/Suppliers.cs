using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Suppliers : BaseEntity
    {
        [Required]
        public string SupplierName { get; set; } = null!;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        // Navigation properties
        public virtual ICollection<SupplierListings> SupplierListings { get; set; } = new HashSet<SupplierListings>();
        public virtual ICollection<FlowerBatches> FlowerBatches { get; set; } = new HashSet<FlowerBatches>();
        public virtual ICollection<PurchaseOrders> PurchaseOrders { get; set; } = new HashSet<PurchaseOrders>();
    }
}
