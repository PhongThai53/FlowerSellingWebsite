namespace FlowerSellingWebsite.Models.Entities
{
    public class Blog : BaseEntity
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Tags {  get; set; }
        public List<string> Images { get; set; }
        public bool Published { get; set; } = true;
        public int UserId { get; set; }
        public int CategoryId { get; set; }

        public FlowerCategories Category { get; set; }
        public Users User { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
