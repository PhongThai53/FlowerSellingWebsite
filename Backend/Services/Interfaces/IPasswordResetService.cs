namespace FlowerSellingWebsite.Services.Interfaces
{
    public interface IPasswordResetService
    {
        string GeneratePasswordResetToken(string email);
        bool ValidatePasswordResetToken(string token, out string email);
        void RemovePasswordResetToken(string token);
        void CleanupExpiredTokens();
    }
}
