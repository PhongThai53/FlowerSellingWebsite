using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Comment : BaseEntity
    {
        [Required]
        public string Content { get; set; } = null!;
        
        public bool IsHide { get; set; } = false;
        
        [Required]
        public int BlogId { get; set; }
        
        public int? ParentId { get; set; }
        
        [Required]
        public int UserId { get; set; }

        // Navigation properties
        public Users User { get; set; } = null!;
        public Blog Blog { get; set; } = null!;
        public Comment? Parent { get; set; }
        public List<Comment> Children { get; set; } = new List<Comment>();
    }
}
