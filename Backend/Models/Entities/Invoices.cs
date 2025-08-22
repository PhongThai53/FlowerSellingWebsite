using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Invoices : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = null!;

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public decimal Subtotal { get; set; }

        [Required]
        public decimal TaxAmount { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        // Customer information
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        // Company information
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyTaxCode { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }

        // Invoice details
        public string? ExportPath { get; set; }
        public DateTime? ExportedAt { get; set; }
        public string? ExportFormat { get; set; } // PDF, Excel, etc.
        public string? Notes { get; set; }

        // Navigation
        public virtual Orders Order { get; set; } = null!;
    }
}