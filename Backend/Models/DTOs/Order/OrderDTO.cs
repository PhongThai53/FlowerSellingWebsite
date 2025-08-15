using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Order
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public bool IsSaleOrder { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public decimal EstimatedTotalAmount { get; set; }
        public decimal? FinalTotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? SupplierNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderDetailDTO>? OrderDetails { get; set; }
    }

    public class OrderDetailDTO
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? FlowerBatchId { get; set; }
        public string? FlowerBatchName { get; set; }
        public int? SupplierListingId { get; set; }
        public string? SupplierListingName { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int? ApprovedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? FinalUnitPrice { get; set; }
        public decimal EstimatedAmount { get; set; }
        public decimal? FinalAmount { get; set; }
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
        public int RequestedQuantity { get; set; }

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
        public int? RequestedQuantity { get; set; }

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
