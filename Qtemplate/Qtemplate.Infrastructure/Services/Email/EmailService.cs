// Qtemplate.Infrastructure/Services/Email/EmailService.cs
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IEmailLogRepository _emailLogRepo;

    public EmailService(IConfiguration config, IEmailLogRepository emailLogRepo)
    {
        _config = config;
        _emailLogRepo = emailLogRepo;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, string template = "")
    {
        var log = new EmailLog
        {
            To = toEmail,
            Subject = subject,
            Body = htmlBody,
            Template = template,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _emailLogRepo.AddAsync(log);

        try
        {
            var smtp = _config.GetSection("Email:Smtp");
            using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]!))
            {
                Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                EnableSsl = true
            };
            using var message = new MailMessage
            {
                From = new MailAddress(smtp["Username"]!, smtp["DisplayName"] ?? "Qtemplate"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            await client.SendMailAsync(message);

            log.Status = "Sent";
            log.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.ErrorMessage = ex.Message;
            log.RetryCount += 1;
        }
        finally
        {
            await _emailLogRepo.UpdateAsync(log);
        }

    }
    public async Task SendDirectAsync(string to, string subject, string body, string? cc = null)
    {
        var smtp = _config.GetSection("Email:Smtp");

        using var client = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]!))
        {
            Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
            EnableSsl = true
        };

        using var message = new MailMessage
        {
            From = new MailAddress(smtp["Username"]!, smtp["DisplayName"] ?? "Qtemplate"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        if (!string.IsNullOrEmpty(cc))
            message.CC.Add(cc);

        await client.SendMailAsync(message);
    }
}