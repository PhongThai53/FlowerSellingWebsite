namespace FlowerSellingWebsite.Models.DTOs.Blog
{
    public class PagedBlogResultDTO
    {
        public List<BlogListDTO> Blogs { get; set; } = new List<BlogListDTO>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
