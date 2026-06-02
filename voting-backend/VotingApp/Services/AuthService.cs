using VotingApp.DTOs.Auth;
using VotingApp.Models;
using VotingApp.Repositories;

namespace VotingApp.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<TokenResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsAsync(request.Email))
        {
            return null;
        }

        var isFirstUser = !await _userRepository.AnyAsync();
        var adminCode = _configuration["AdminSettings:SecretCode"];
        var isAdmin = isFirstUser || (!string.IsNullOrEmpty(request.AdminCode) && request.AdminCode == adminCode);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            IsAdmin = isAdmin
        };

        await _userRepository.CreateAsync(user);
        return _jwtService.GenerateTokens(user);
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return _jwtService.GenerateTokens(user);
    }

    public async Task<TokenResponse?> RefreshTokensAsync(string refreshToken)
    {
        return await _jwtService.RefreshTokensAsync(refreshToken);
    }

    public async Task RevokeTokensAsync(string refreshToken)
    {
        await _jwtService.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            IsAdmin = user.IsAdmin
        };
    }
}
