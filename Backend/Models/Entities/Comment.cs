namespace FlowerSellingWebsite.Models.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; set; }
        public bool IsHide { get; set; } = false;
        public int BlogId { get; set; }
        public int ParentId { get; set; }

        public Blog Blog { get; set; }
        public Comment? Parent { get; set; }
        public List<Comment> Children { get; set; } = new List<Comment>();

    }
}
