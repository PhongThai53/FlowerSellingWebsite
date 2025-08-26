using FlowerSellingWebsite.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.DTOs
{
    public class FlowerResponse
    {
        public string Name { get; set; }
        public string? Description { get; set; }


        public int? FlowerCategoryId { get; set; }


        [ForeignKey("FlowerType")]
        public int FlowerTypeId { get; set; }


        [ForeignKey("FlowerColor")]
        public int FlowerColorId { get; set; }

        public string? Size { get; set; }


        public int ShelfLifeDays { get; set; }

        // Navigation properties
        public FlowerCategoryResponse? FlowerCategory { get; set; }
        public FlowerTypeResponse? FlowerType { get; set; }
        public FlowerColorResponse? FlowerColor { get; set; }
    }

    public class FlowerCategoryResponse
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class FlowerTypeResponse
    {
        public int Id { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
    }

    public class FlowerColorResponse
    {
        public int Id { get; set; }
        public string? ColorName { get; set; }
        public string? HexCode { get; set; }
        public string? Description { get; set; }
    }
}
