using FlowerSellingWebsite.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Blog : BaseEntity
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = null!;
        
        [Required]
        public string Content { get; set; } = null!;
        
        public string Tags { get; set; } = string.Empty;
        
        public List<string> Images { get; set; } = new List<string>();
        
        public BlogStatus Status { get; set; } = BlogStatus.Draft;
        
        public string? RejectionReason { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int CategoryId { get; set; }

        // Navigation properties
        public FlowerCategories Category { get; set; } = null!;
        public Users User { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
