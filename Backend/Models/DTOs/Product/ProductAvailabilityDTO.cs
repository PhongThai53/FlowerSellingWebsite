using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs.Product
{
    public class ProductAvailabilityDTO
    {
        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("requestedQuantity")]
        public int RequestedQuantity { get; set; }

        [JsonPropertyName("maxAvailableQuantity")]
        public int MaxAvailableQuantity { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("flowerRequirements")]
        public List<FlowerRequirementInfo> FlowerRequirements { get; set; } = new List<FlowerRequirementInfo>();
    }

    public class FlowerRequirementInfo
    {
        [JsonPropertyName("flowerId")]
        public int FlowerId { get; set; }

        [JsonPropertyName("flowerName")]
        public string FlowerName { get; set; } = string.Empty;

        [JsonPropertyName("quantityNeeded")]
        public int QuantityNeeded { get; set; }

        [JsonPropertyName("totalQuantityNeeded")]
        public int TotalQuantityNeeded { get; set; }

        [JsonPropertyName("availableQuantity")]
        public int AvailableQuantity { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }

        [JsonPropertyName("shortage")]
        public int Shortage { get; set; }
    }
}


