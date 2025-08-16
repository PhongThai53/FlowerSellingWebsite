using FlowerSellingWebsite.Models.Enums;

namespace FlowerSellingWebsite.Models.DTOs.Blog
{
    public class BlogDTO
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Tags { get; set; } = string.Empty;
        public List<string> Images { get; set; } = new List<string>();
        public BlogStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int CommentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
