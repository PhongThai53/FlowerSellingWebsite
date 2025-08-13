using System.ComponentModel.DataAnnotations;

namespace FlowerSellingWebsite.Models.Entities
{
    public class SystemLog : BaseEntity
    {
        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string? Action { get; set; }

        public Guid RecordId { get; set; }

        [MaxLength(100)]
        public string? TableName { get; set; }

        [MaxLength(45)]
        public string? IPAddress { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual Order? Order { get; set; }
    }
}
