using FlowerSellingWebsite.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs.Product
{
    public class CreateProductDTO
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters")]
        public string Name { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
        public string Url { get; set; } = null!;

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be 0 or greater")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Category ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int CategoryId { get; set; }

        public List<ProductPhotos>? ProductPhotos { get; set; } = new List<ProductPhotos>();

        public List<ProductFlowers> ProductFlowers { get; set; } = new List<ProductFlowers>();
    }
}
