using VotingApp.DTOs.Auth;
using VotingApp.Models;

namespace VotingApp.Services;

public interface IAuthService
{
    Task<TokenResponse?> RegisterAsync(RegisterRequest request);
    Task<TokenResponse?> LoginAsync(LoginRequest request);
    Task<TokenResponse?> RefreshTokensAsync(string refreshToken);
    Task RevokeTokensAsync(string refreshToken);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
    Task<bool> VerifyEmailAsync(string email, string code);
    Task<bool> ResendVerificationEmailAsync(string email);
}
