using System.Security.Claims;

namespace ProjectGreenLens.Services.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(string userId, string role);
        Task<ClaimsPrincipal?> ValidateTokenAsync(string token);
    }
}
