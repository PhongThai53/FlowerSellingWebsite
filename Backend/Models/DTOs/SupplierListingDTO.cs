using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class SupplierListingDTO
    {
        public int SupplierId { get; set; }

        public int FlowerId { get; set; }

        [Required]
        public int AvailableQuantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int ShelfLifeDays { get; set; }

        [Required]
        public int MinOrderQty { get; set; }

        [Required]
        public string Status { get; set; } = "pending";
    }
}
