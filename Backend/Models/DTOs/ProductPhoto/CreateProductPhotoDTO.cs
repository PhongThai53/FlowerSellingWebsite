namespace FlowerSellingWebsite.Models.DTOs.ProductPhoto
{
    public class CreateProductPhotoDTO
    {
        public int ProductId { get; set; }
        public string Url { get; set; } = null!;
        public bool IsPrimary { get; set; }
    }
}
