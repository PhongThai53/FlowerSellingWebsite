using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Orders : BaseEntity
    {
        [Required]
        public string OrderNumber { get; set; } = null!;

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? CancelledDate { get; set; }

        [Required]
        public string Status { get; set; } = "Created"; // Created, Processing, Shipped, Delivered, Cancelled

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded

        [MaxLength(500)]
        public string? ShippingAddress { get; set; }

        [MaxLength(500)]
        public string? BillingAddress { get; set; }

        public string? Notes { get; set; }
        public string? SupplierNotes { get; set; }

        // Checkout information
        [MaxLength(100)]
        public string? CustomerFirstName { get; set; }

        [MaxLength(100)]
        public string? CustomerLastName { get; set; }

        [MaxLength(100)]
        public string? CustomerEmail { get; set; }

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }

        [MaxLength(100)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? State { get; set; }

        [MaxLength(20)]
        public string? Postcode { get; set; }

        [MaxLength(500)]
        public string? StreetAddress { get; set; }

        [MaxLength(500)]
        public string? StreetAddress2 { get; set; }

        // Invoice related
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public bool IsInvoiceExported { get; set; } = false;
        public string? InvoiceExportPath { get; set; }

        // Audit
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }

        // Navigation
        public virtual Users? Customer { get; set; }
        public virtual ICollection<OrderDetails> OrderDetails { get; set; } = new HashSet<OrderDetails>();
        public virtual ICollection<Payments> Payments { get; set; } = new HashSet<Payments>();
        public virtual ICollection<Deliveries> Deliveries { get; set; } = new HashSet<Deliveries>();
        public virtual ICollection<Invoices> Invoices { get; set; } = new HashSet<Invoices>();
    }
}