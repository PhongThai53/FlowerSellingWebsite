using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Order
{
    public class OrderDTO
    {
        [JsonPropertyName("order_number")]
        public string OrderNumber { get; set; } = null!;

        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("order_date")]
        public DateTime OrderDate { get; set; }

        [JsonPropertyName("required_date")]
        public DateTime? RequiredDate { get; set; }

        [JsonPropertyName("shipped_date")]
        public DateTime? ShippedDate { get; set; }

        [JsonPropertyName("cancelled_date")]
        public DateTime? CancelledDate { get; set; }

        [JsonPropertyName("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("discount_amount")]
        public decimal DiscountAmount { get; set; }

        [JsonPropertyName("tax_amount")]
        public decimal TaxAmount { get; set; }

        [JsonPropertyName("shipping_fee")]
        public decimal ShippingFee { get; set; }

        [JsonPropertyName("total_amount")]
        public decimal TotalAmount { get; set; } // subtotal - discount + tax + ship

        [JsonPropertyName("payment_status")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        [JsonPropertyName("shipping_address")]
        public string? ShippingAddress { get; set; }

        [JsonPropertyName("billing_address")]
        public string? BillingAddress { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("supplier_notes")]
        public string? SupplierNotes { get; set; }

        [MaxLength(100)]
        [JsonPropertyName("created_by")]
        public string? CreatedBy { get; set; }

        [MaxLength(100)]
        [JsonPropertyName("updated_by")]
        public string? UpdatedBy { get; set; }

        [JsonPropertyName("order_details")]
        public List<OrderDetailDTO>? OrderDetails { get; set; }
    }

    public enum OrderStatus
    {
        Pending,     
        Confirmed,   
        Preparing,   
        Delivering,  
        Delivered,   
        Cancelled   
    }

    public enum PaymentStatus
    {
        Unpaid,      
        Pending,     
        Paid,         
        Failed,       
        Refunded
    }


    public class OrderDetailDTO
    {
        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }

        [JsonPropertyName("product_id")]
        public int ProductId { get; set; }

        [JsonPropertyName("item_name")]
        public string? ItemName { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("line_total")]
        public decimal LineTotal { get; set; }

        [JsonPropertyName("expiration_date")]
        public DateTime? ExpirationDate { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }
    }

    public class CreateOrderDTO
    {
        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        public bool IsSaleOrder { get; set; } = true;

        public int? CustomerId { get; set; }

        public int? SupplierId { get; set; }

        public DateTime? RequiredDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? SupplierNotes { get; set; }

        [Required]
        public List<CreateOrderDetailDTO> OrderDetails { get; set; } = new List<CreateOrderDetailDTO>();
    }

    public class CreateOrderDetailDTO
    {
        public int? ProductId { get; set; }

        public int? FlowerBatchId { get; set; }

        public int? SupplierListingId { get; set; }

        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RequestedQuantity must be positive")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be positive")]
        public decimal UnitPrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateOrderDTO
    {
        [StringLength(50)]
        public string? OrderNumber { get; set; }

        public DateTime? RequiredDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? SupplierNotes { get; set; }

        public string? Status { get; set; }
    }

    public class UpdateOrderDetailDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "RequestedQuantity must be positive")]
        public int? Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "ApprovedQuantity must be non-negative")]
        public int? ApprovedQuantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be positive")]
        public decimal? UnitPrice { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "FinalUnitPrice must be positive")]
        public decimal? FinalUnitPrice { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class OrderFilterDTO
    {
        public string? SearchTerm { get; set; }
        public bool? IsSaleOrder { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "OrderDate";
        public bool SortDescending { get; set; } = true;
    }

    public class PagedOrdersResultDTO
    {
        public List<OrderDTO> Orders { get; set; } = new List<OrderDTO>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
