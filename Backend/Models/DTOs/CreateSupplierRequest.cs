using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class CreateSupplierRequest
    {
        public string SupplierName { get; set; } = null!;
        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
    }
}
