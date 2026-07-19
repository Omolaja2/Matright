using System.Net;
using System.Net.Mail;

namespace PharMarket.Services;

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "PharMarket";
}

public interface IEmailService
{
    Task SendCredentialsAsync(string toEmail, string fullName, string loginEmail, string password, string role);
    void SendCredentials(string toEmail, string fullName, string email, string password, string role);
}

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtp;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smtp = configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings();
        _logger = logger;
    }

    public async Task SendCredentialsAsync(string toEmail, string fullName, string loginEmail, string password, string role)
    {
        try
        {
            var subject = $"Your PharMarket Account - {role} Access";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
                    <div style='background: #1a73e8; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0;'>
                        <h2 style='margin: 0;'>PharMarket</h2>
                        <p style='margin: 5px 0 0;'>Account Created</p>
                    </div>
                    <div style='padding: 25px; border: 1px solid #ddd; border-top: none; border-radius: 0 0 8px 8px;'>
                        <p>Hello <strong>{WebUtility.HtmlEncode(fullName)}</strong>,</p>
                        <p>Your <strong>{WebUtility.HtmlEncode(role)}</strong> account has been created. Here are your login details:</p>
                        <div style='background: #f5f5f5; padding: 15px; border-radius: 6px; margin: 15px 0;'>
                            <p style='margin: 5px 0;'><strong>Email:</strong> {WebUtility.HtmlEncode(loginEmail)}</p>
                            <p style='margin: 5px 0;'><strong>Password:</strong> <code style='background:#e8e8e8;padding:2px 6px;border-radius:3px;'>{WebUtility.HtmlEncode(password)}</code></p>
                        </div>
                        <p style='color: #d32f2f; font-size: 13px;'>Please change your password after your first login for security.</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                        <p style='color: #888; font-size: 12px;'>This is an automated message from PharMarket. Do not share these credentials with anyone.</p>
                    </div>
                </body>
                </html>";

            using var message = new MailMessage();
            message.From = new MailAddress(_smtp.FromEmail, _smtp.FromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = _smtp.EnableSsl
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email} for user {Name}", toEmail, fullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
        }
    }

    public void SendCredentials(string toEmail, string fullName, string email, string password, string role)
    {
        SendCredentialsAsync(toEmail, fullName, email, password, role).GetAwaiter().GetResult();
    }
}
