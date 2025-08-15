namespace FlowerSellingWebsite.Models.DTOs
{
    public class UserDTO
    {
        public Guid PublicId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        //public bool IsCustomer { get; set; }
        //public bool IsSupplier { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

