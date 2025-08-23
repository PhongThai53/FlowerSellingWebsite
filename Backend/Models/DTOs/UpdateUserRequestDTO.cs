namespace FlowerSellingWebsite.Models.DTOs
{
    public class UpdateUserRequestDTO
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? RoleName { get; set; }
    }
}
