using MailKit.Net.Smtp;
using MimeKit;
using VotingApp.Models;

namespace VotingApp.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(User user, string verificationCode);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(User user, string verificationCode)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var smtpHost = emailSettings["SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "1025");
        var fromEmail = emailSettings["FromEmail"] ?? "noreply@votingapp.local";
        var fromName = emailSettings["FromName"] ?? "Voting App";
        var enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "false");
        var username = emailSettings["Username"];
        var password = emailSettings["Password"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(user.FullName, user.Email));
        message.Subject = "Код подтверждения email - Voting App";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $@"
                <h2>Подтверждение email</h2>
                <p>Здравствуйте, {user.FullName}!</p>
                <p>Ваш код подтверждения email:</p>
                <div style=""font-size: 32px; font-weight: bold; letter-spacing: 8px; padding: 20px; background-color: #f8f9fa; border-radius: 8px; text-align: center; margin: 20px 0;"">{verificationCode}</div>
                <p>Введите этот код на странице подтверждения email.</p>
                <p>Код действителен в течение 24 часов.</p>
                <hr>
                <p style=""color: #666;"">Если вы не регистрировались на Voting App, просто проигнорируйте это письмо.</p>
            ",
            TextBody = $@"
                Подтверждение email
                Здравствуйте, {user.FullName}!
                Ваш код подтверждения email: {verificationCode}
                Введите этот код на странице подтверждения email.
                Код действителен в течение 24 часов.
                Если вы не регистрировались на Voting App, просто проигнорируйте это письмо.
            "
        };
        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            client.Timeout = 10000;
            var options = enableSsl ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None;
            await client.ConnectAsync(smtpHost, smtpPort, options);
            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Verification email with code sent to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);
            throw;
        }
    }
}
