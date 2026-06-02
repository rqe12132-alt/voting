using VotingApp.Models;

namespace VotingApp.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);
    Task RevokeAsync(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(Guid userId);
}
