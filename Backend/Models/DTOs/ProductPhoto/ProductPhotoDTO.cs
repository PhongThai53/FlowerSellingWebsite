namespace FlowerSellingWebsite.Models.DTOs.ProductPhoto
{
    public class ProductPhotoDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Url { get; set; } = null!;
        public bool IsPrimary { get; set; }
    }
}
