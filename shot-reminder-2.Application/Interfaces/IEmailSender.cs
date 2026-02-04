
namespace shot_reminder_2.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
}
