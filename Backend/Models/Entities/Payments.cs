using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerSellingWebsite.Models.Entities
{
    public class Payments : BaseEntity
    {
        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        public string MethodName { get; set; } = null!;   // Trực tiếp lưu tên phương thức

        public string? Description { get; set; }          // Mô tả thêm (nếu cần)

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string Status { get; set; } = null!;       // Pending, Completed, Failed...

        // Navigation
        public virtual Orders Order { get; set; } = null!;
    }
}
