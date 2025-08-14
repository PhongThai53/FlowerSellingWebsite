namespace FlowerSellingWebsite.Models.DTOs.Category
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCategoryDTO
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Color { get; set; }
        public bool IsActive { get; set; }
    }
}
