using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.SupplierListing
{
    public class CreateSupplierListingDTO
    {
        [Required]
        public int SupplierId { get; set; }
        
        [Required]
        public int FlowerId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int AvailableQuantity { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal UnitPrice { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số ngày bảo quản phải lớn hơn 0")]
        public int ShelfLifeDays { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng đặt hàng tối thiểu phải lớn hơn 0")]
        public int MinOrderQty { get; set; }
        
        public string Status { get; set; } = "Pending";
    }

    public class SupplierListingResponseDTO
    {
        public int SupplierId { get; set; }
        public int FlowerId { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int ShelfLifeDays { get; set; }
        public int MinOrderQty { get; set; }
        public string Status { get; set; }
        
        // Thêm thông tin hoa để hiển thị
        public string FlowerName { get; set; }
        public string FlowerDescription { get; set; }
        public string FlowerSize { get; set; }
        public string CategoryName { get; set; }
        public string TypeName { get; set; }
        public string ColorName { get; set; }
    }

    public class SupplierListingListRequestDTO
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
