
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using shot_reminder_2.Application.Interfaces;

namespace shot_reminder_2.Infrastructure.Services;

public class GmailSmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailSmtpEmailSender> _logger;

    public GmailSmtpEmailSender(IConfiguration configuration, ILogger<GmailSmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        try
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var smtpUsername = _configuration["Smtp:Username"];
            var smtpPassword = _configuration["Smtp:Password"];
            var senderEmail = _configuration["Smtp:SenderEmail"];
            var senderName = _configuration["Smtp:SenderName"];

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) ||
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(senderEmail))
            {
                throw new InvalidOperationException("SMTP configuration is incomplete.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName ?? "ShotReminder", senderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // Connect to Gmail SMTP server
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

            // Authenticate
            await client.AuthenticateAsync(smtpUsername, smtpPassword);

            // Send the email
            await client.SendAsync(message);

            // Disconnect
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }
}
