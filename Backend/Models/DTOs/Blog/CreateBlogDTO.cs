using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Blog
{
    public class CreateBlogDTO
    {
        [Required]
        [MaxLength(250)]
        public string Title { get; set; } = null!;
        
        [Required]
        public string Content { get; set; } = null!;
        
        public string Tags { get; set; } = string.Empty;
        
        [Required]
        public int CategoryId { get; set; }
    }
}
