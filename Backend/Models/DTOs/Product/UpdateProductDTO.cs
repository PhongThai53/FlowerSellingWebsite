namespace FlowerSellingWebsite.Models.DTOs.Product
{
    public class UpdateProductDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryId { get; set; }
    }
}
