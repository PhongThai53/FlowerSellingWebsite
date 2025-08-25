namespace FlowerSellingWebsite.Models.DTOs.Cart
{
    public class CartValidationResultDTO
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<CartItemValidationInfo> CartItems { get; set; } = new List<CartItemValidationInfo>();
        public int MaxQuantityAllowed { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartItemValidationInfo
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int MaxAvailableQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

