using FlowerSellingWebsite.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class CreateFlowerWithSupplierRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("flower_category_id")]
        public int? FlowerCategoryId { get; set; }

        [JsonPropertyName("flower_type_id")]
        public int FlowerTypeId { get; set; }

        [JsonPropertyName("flower_color_id")]
        public int FlowerColorId { get; set; }

        [JsonPropertyName("size")]
        public string? Size { get; set; }

        [JsonPropertyName("flower_images")]
        public FlowerImageRequest FlowerImageRequests { get; set; }
    }

    public class CreateSupplierListingRequest
    {
        [JsonPropertyName("shelf_life_days")]
        public int ShelfLifeDays { get; set; }

        [JsonPropertyName("min_order_quantity")]
        public int MinOrderQty { get; set; }

        [JsonPropertyName("product_flowers")]
        public ProductFlowerRequest ProductFlowers { get; set; }

        [JsonPropertyName("flower_prices")]
        public FlowerPriceRequest FlowerPriceRequests { get; set; }
    }

    public class ProductFlowerRequest
    {
        [JsonPropertyName("flower_id")]
        public int FlowerId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public class FlowerImageRequest
    {
        [Required]
        [StringLength(500)]
        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [StringLength(50)]
        [JsonPropertyName("image_type")]
        public string? ImageType { get; set; }

        [Required]
        [JsonPropertyName("is_primary")]
        public bool IsPrimary { get; set; }

        [Required]
        [JsonPropertyName("display_order")]
        public int DisplayOrder { get; set; }
    }

    public class FlowerPriceRequest
    {
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(3)]
        [JsonPropertyName("currency")]
        public required string Currency { get; set; } = "VND";

        [Required]
        [JsonPropertyName("effective_date")]
        public DateTime EffectiveDate { get; set; }

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [StringLength(20)]
        [JsonPropertyName("price_type")]
        public string? PriceType { get; set; }
    }
}
