using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Comment
{
    public class CreateCommentDTO
    {
        [Required]
        public string Content { get; set; } = null!;
        
        [Required]
        public int BlogId { get; set; }
        
        public int? ParentId { get; set; }
    }
}
