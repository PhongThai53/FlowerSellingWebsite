namespace FlowerSellingWebsite.Models.DTOs.Comment
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public Guid PublicId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsHide { get; set; }
        public int BlogId { get; set; }
        public int? ParentId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string UserFullName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CommentDTO> Children { get; set; } = new List<CommentDTO>();
    }
}
