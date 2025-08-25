namespace FlowerSellingWebsite.Models.DTOs.Product
{
    public class ProductAvailabilityWithPricingDTO
    {
        public bool IsAvailable { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int MaxAvailableQuantity { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal CalculatedUnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public List<SupplierPriceBreakdown> SupplierBreakdown { get; set; } = new List<SupplierPriceBreakdown>();
    }

    public class SupplierPriceBreakdown
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int FlowerId { get; set; }
        public string FlowerName { get; set; } = string.Empty;
        public int QuantityNeeded { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}

