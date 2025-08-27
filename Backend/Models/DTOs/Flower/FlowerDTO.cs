namespace FlowerSellingWebsite.Models.DTOs.Flower
{
    public class FlowerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Size { get; set; }
        public int ShelfLifeDays { get; set; }
        
        // Simple properties instead of navigation objects
        public int? FlowerCategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int FlowerTypeId { get; set; }
        public string? TypeName { get; set; }
        public int FlowerColorId { get; set; }
        public string? ColorName { get; set; }
    }
}

