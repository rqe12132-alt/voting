using VotingApp.DTOs.Auth;
using VotingApp.Models;

namespace VotingApp.Services;

public interface IJwtService
{
    TokenResponse GenerateTokens(User user);
    Task<TokenResponse?> RefreshTokensAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Guid? GetUserIdFromToken(string token);
}
