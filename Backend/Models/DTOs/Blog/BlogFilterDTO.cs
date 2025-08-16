using FlowerSellingWebsite.Models.Enums;

namespace FlowerSellingWebsite.Models.DTOs.Blog
{
    public class BlogFilterDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public string? Search { get; set; } // Search in Title, Content, Tags
        public int? CategoryId { get; set; }
        public int? UserId { get; set; }
        public BlogStatus? Status { get; set; }
        public string? Tags { get; set; } // Filter by tags (contains)
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string SortBy { get; set; } = "CreatedAt"; // CreatedAt, UpdatedAt, Title
        public string SortOrder { get; set; } = "desc"; // asc, desc
    }
}
