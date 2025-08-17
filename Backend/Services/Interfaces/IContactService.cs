namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IContactService
    {
        Task SendContactEmailAsync(Models.DTOs.ContactFormDTO contactForm);
    }
}
