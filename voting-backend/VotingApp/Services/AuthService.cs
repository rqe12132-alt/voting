using VotingApp.DTOs.Auth;
using VotingApp.Models;
using VotingApp.Repositories;

namespace VotingApp.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPersonalIdRepository _personalIdRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, IPersonalIdRepository personalIdRepository, IJwtService jwtService, IEmailService emailService, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _personalIdRepository = personalIdRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    private static string GenerateVerificationCode()
    {
        return new Random().Next(100000, 999999).ToString("D6");
    }

    public async Task<TokenResponse?> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsAsync(request.Email))
        {
            throw new ArgumentException("Пользователь с таким email уже существует");
        }

        // Validate and consume personal number
        var personalNumber = request.PersonalNumber?.Trim().ToUpper() ?? "";
        if (string.IsNullOrEmpty(personalNumber))
        {
            throw new ArgumentException("Идентификационный номер паспорта обязателен");
        }

        var personalId = await _personalIdRepository.GetByNumberAsync(personalNumber);
        if (personalId == null)
        {
            throw new ArgumentException("Идентификационный номер не найден в базе данных");
        }
        if (personalId.IsUsed)
        {
            throw new ArgumentException("Идентификационный номер уже использован");
        }

        var isFirstUser = !await _userRepository.AnyAsync();
        var adminCode = _configuration["AdminSettings:SecretCode"];
        var isAdmin = isFirstUser || (!string.IsNullOrEmpty(request.AdminCode) && request.AdminCode == adminCode);

        var verificationCode = GenerateVerificationCode();
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            IsAdmin = isAdmin,
            EmailVerified = false,
            EmailVerificationToken = verificationCode,
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        await _userRepository.CreateAsync(user);

        // Mark personal number as used and link to user
        personalId.IsUsed = true;
        personalId.User = user;
        await _personalIdRepository.UpdateAsync(personalId);

        await _emailService.SendVerificationEmailAsync(user, verificationCode);

        return _jwtService.GenerateTokens(user);
    }

    public async Task<TokenResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        _logger.LogInformation("Login user {Email} EmailVerified={EmailVerified}", user.Email, user.EmailVerified);
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

        var personalId = await _personalIdRepository.GetByUserIdAsync(userId);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PersonalNumber = personalId?.Number ?? "",
            IsAdmin = user.IsAdmin,
            EmailVerified = user.EmailVerified
        };
    }

    public async Task<bool> VerifyEmailAsync(string email, string code)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Verification email not found: {Email}", email);
            return false;
        }

        if (user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Verification code expired for user {Email}", user.Email);
            return false;
        }

        if (user.EmailVerificationToken != code)
        {
            _logger.LogWarning("Invalid verification code for user {Email}: expected {Expected}, got {Got}", user.Email, user.EmailVerificationToken, code);
            return false;
        }

        if (user.EmailVerified)
        {
            return true;
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;
        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("Email verified for user {Email}", user.Email);
        return true;
    }

    public async Task<bool> ResendVerificationEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null || user.EmailVerified)
        {
            return false;
        }

        var verificationCode = GenerateVerificationCode();
        user.EmailVerificationToken = verificationCode;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        await _userRepository.UpdateAsync(user);

        await _emailService.SendVerificationEmailAsync(user, verificationCode);
        return true;
    }
}
