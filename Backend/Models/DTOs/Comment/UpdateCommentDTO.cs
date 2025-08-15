using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Comment
{
    public class UpdateCommentDTO
    {
        [Required]
        public string Content { get; set; } = null!;
    }
}
