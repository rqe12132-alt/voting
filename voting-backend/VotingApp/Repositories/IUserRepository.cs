using VotingApp.Models;

namespace VotingApp.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsAsync(string email);
    Task<bool> AnyAsync();
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
}
