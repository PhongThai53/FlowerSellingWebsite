namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IEmailVerificationService
    {
        string GenerateVerificationToken(string email);
        bool ValidateVerificationToken(string token, out string email);
        void RemoveVerificationToken(string token);
        bool IsEmailPendingVerification(string email);
        void CleanupExpiredTokens();
    }
}
